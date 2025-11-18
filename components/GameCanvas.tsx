
import React, { useRef, useEffect, useState, useCallback } from 'react';
import { Entity, EntityType, GameStatus, TileType, Vector2D, LevelData, Player } from '../types';
import { TILE_SIZE, SCALE, SCREEN_WIDTH, SCREEN_HEIGHT, GRAVITY, FRICTION, ACCELERATION, MAX_WALK_SPEED, MAX_RUN_SPEED, JUMP_FORCE, COLORS, MARIO_SPRITE_STAND, GOOMBA_SPRITE, BOUNCE_FORCE, MARIO_SPRITE_BIG, MUSHROOM_SPRITE, FLOWER_SPRITE, FIREBALL_SPRITE } from '../constants';
import { audioService } from '../services/audioService';

interface GameCanvasProps {
  status: GameStatus;
  levelData: LevelData;
  onScore: (points: number) => void;
  onCoin: () => void;
  onDie: () => void;
  onWin: () => void;
}

interface Keys {
  left: boolean;
  right: boolean;
  jump: boolean;
  run: boolean;
}

const GameCanvas: React.FC<GameCanvasProps> = ({ status, levelData, onScore, onCoin, onDie, onWin }) => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const requestRef = useRef<number>(0);
  const frameCountRef = useRef<number>(0);
  const flagEntityRef = useRef<Entity | null>(null);
  const cutsceneState = useRef<'none' | 'sliding' | 'walking_castle'>('none');
  
  const gameState = useRef({
    player: {
      id: 0,
      type: EntityType.PLAYER,
      pos: { x: 50, y: 100 },
      vel: { x: 0, y: 0 },
      width: 12, 
      height: 16,
      dead: false,
      grounded: false,
      direction: 1 as 1 | -1,
      powerMode: 'small' as 'small' | 'big' | 'fire',
      isJumping: false,
      invulnerable: 0
    } as Player,
    camera: { x: 0 },
    entities: [] as Entity[],
    particles: [] as any[],
    map: [] as number[][]
  });

  const keys = useRef<Keys>({ left: false, right: false, jump: false, run: false });

  // Helper function to find safe starting position for Mario
  const findSafeStartPosition = (map: number[][]): { x: number, y: number } => {
    const START_X = 50; // Preferred start X position
    const START_TILE_X = Math.floor(START_X / TILE_SIZE);
    const GROUND_TILES = [TileType.GROUND, TileType.BRICK, TileType.QUESTION_BLOCK, TileType.HARD_BLOCK, TileType.PIPE_L, TileType.PIPE_R, TileType.PIPE_TOP_L, TileType.PIPE_TOP_R];
    const MARIO_HEIGHT = 16;
    
    // Check columns from start position, going left if needed
    for (let offset = 0; offset < 10; offset++) {
      const checkX = Math.max(0, START_TILE_X - offset);
      
      // Find the TOP of the ground tile at this X position
      // We need to find the highest ground tile (lowest y value) that has ground
      // Search from top down to find the first (highest) ground tile
      let groundTileY = -1;
      for (let y = 0; y < map.length; y++) {
        if (map[y] && GROUND_TILES.includes(map[y][checkX])) {
          // Found ground - this is the topmost ground tile at this column
          groundTileY = y;
          break; // Found the topmost ground tile, no need to continue
        }
      }
      
      if (groundTileY >= 0) {
        // Found ground tile at row groundTileY
        // Ground tile's top edge is at groundTileY * TILE_SIZE
        // Mario's bottom should be at groundTileY * TILE_SIZE
        // So Mario's top should be at groundTileY * TILE_SIZE - MARIO_HEIGHT
        const groundTopY = groundTileY * TILE_SIZE;
        const marioY = groundTopY - MARIO_HEIGHT;
        
        // Double-check: Mario should be above ground, not inside it
        if (marioY < groundTopY) {
          return { x: checkX * TILE_SIZE + 2, y: marioY };
        }
      }
    }
    
    // Fallback: place at bottom of screen (above ground level)
    // Use row 12 (one row above typical ground rows 13-14)
    const fallbackY = 12 * TILE_SIZE - MARIO_HEIGHT;
    return { x: START_X, y: fallbackY };
  };

  useEffect(() => {
    if (status === GameStatus.PLAYING) {
      gameState.current.map = JSON.parse(JSON.stringify(levelData.map)); // Deep copy map
      gameState.current.entities = levelData.entities.map(e => ({...e, grounded: false}));
      
      // Find safe starting position
      const startPos = findSafeStartPosition(gameState.current.map);
      
      gameState.current.player = {
        ...gameState.current.player,
        pos: startPos,
        vel: { x: 0, y: 0 },
        dead: false,
        grounded: false,
        powerMode: 'small',
        height: 16,
        invulnerable: 0
      };
      gameState.current.camera.x = 0;
      gameState.current.particles = [];
      flagEntityRef.current = null;
      cutsceneState.current = 'none';

      // Find flag in map to create the flag entity for animation
      const map = gameState.current.map;
      for(let y=0; y<map.length; y++) {
        for(let x=0; x<map[y].length; x++) {
          if (map[y][x] === TileType.FLAG) {
             flagEntityRef.current = {
               id: 9999,
               type: EntityType.FLAG,
               pos: { x: x * TILE_SIZE + 6, y: y * TILE_SIZE }, // Visual align
               vel: { x: 0, y: 0 },
               width: 4, height: 16, dead: false, grounded: false, direction: 1
             };
             map[y][x] = TileType.AIR; // Remove from tilemap so we can animate it
          }
        }
      }

      audioService.init();
    }
  }, [status, levelData]);

  const shootFireball = () => {
     const player = gameState.current.player;
     if (player.powerMode !== 'fire') return;

     // Limit active fireballs
     const fireballCount = gameState.current.entities.filter(e => e.type === EntityType.FIREBALL).length;
     if (fireballCount >= 2) return;

     audioService.playFireball();
     gameState.current.entities.push({
         id: Date.now() + Math.random(),
         type: EntityType.FIREBALL,
         pos: { x: player.pos.x + (player.direction === 1 ? 12 : -4), y: player.pos.y + 8 },
         vel: { x: player.direction * 4, y: 2 }, // Slower fireball speed (was 6)
         width: 8, height: 8, dead: false, grounded: false, direction: player.direction
     });
  };

  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (status !== GameStatus.PLAYING || cutsceneState.current !== 'none') return;
      
      switch (e.code) {
        case 'KeyA': case 'ArrowLeft': keys.current.left = true; break;
        case 'KeyD': case 'ArrowRight': keys.current.right = true; break;
        case 'KeyW': case 'ArrowUp': case 'Space': 
          if (!keys.current.jump && gameState.current.player.grounded) {
             gameState.current.player.vel.y = -JUMP_FORCE;
             gameState.current.player.grounded = false;
             gameState.current.player.isJumping = true;
             audioService.playJump();
          }
          keys.current.jump = true; 
          break;
        case 'ShiftLeft': case 'ShiftRight': 
          if (!keys.current.run && !e.repeat) {
             shootFireball(); // Shoot on initial press
          }
          keys.current.run = true; 
          break;
      }
    };

    const handleKeyUp = (e: KeyboardEvent) => {
      switch (e.code) {
        case 'KeyA': case 'ArrowLeft': keys.current.left = false; break;
        case 'KeyD': case 'ArrowRight': keys.current.right = false; break;
        case 'KeyW': case 'ArrowUp': case 'Space': 
          keys.current.jump = false; 
          if (gameState.current.player.vel.y < -2.5) { // Variable jump height cutoff
             gameState.current.player.vel.y = -2.5; 
          }
          break;
        case 'ShiftLeft': case 'ShiftRight': keys.current.run = false; break;
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    window.addEventListener('keyup', handleKeyUp);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
      window.removeEventListener('keyup', handleKeyUp);
    };
  }, [status]);

  const isSolid = (t: number) => {
    return [TileType.GROUND, TileType.BRICK, TileType.QUESTION_BLOCK, TileType.QUESTION_BLOCK_HIT, TileType.HARD_BLOCK, TileType.PIPE_L, TileType.PIPE_R, TileType.PIPE_TOP_L, TileType.PIPE_TOP_R, TileType.INVISIBLE_BLOCK].includes(t);
  };

  const checkCollision = (rect1: {pos: Vector2D, width: number, height: number}, rect2: {pos: Vector2D, width: number, height: number}) => {
    return (rect1.pos.x < rect2.pos.x + rect2.width &&
            rect1.pos.x + rect1.width > rect2.pos.x &&
            rect1.pos.y < rect2.pos.y + rect2.height &&
            rect1.pos.y + rect1.height > rect2.pos.y);
  };

  const resolveXCollision = (entity: Entity, map: number[][]) => {
    const startY = Math.floor(entity.pos.y / TILE_SIZE);
    const endY = Math.floor((entity.pos.y + entity.height - 0.1) / TILE_SIZE);
    
    if (entity.vel.x > 0) { // Moving Right
      const rightX = Math.floor((entity.pos.x + entity.width) / TILE_SIZE);
      if (isBlocked(rightX, startY, map) || isBlocked(rightX, endY, map)) {
        entity.pos.x = rightX * TILE_SIZE - entity.width;
        entity.vel.x = 0;
        if (entity.type === EntityType.GOOMBA || entity.type === EntityType.MUSHROOM) entity.direction *= -1;
        if (entity.type === EntityType.FIREBALL) { entity.dead = true; spawnParticles(entity.pos.x, entity.pos.y, COLORS.FIRE_RED); }
      }
    } else if (entity.vel.x < 0) { // Moving Left
      const leftX = Math.floor(entity.pos.x / TILE_SIZE);
      if (isBlocked(leftX, startY, map) || isBlocked(leftX, endY, map)) {
        entity.pos.x = (leftX + 1) * TILE_SIZE;
        entity.vel.x = 0;
        if (entity.type === EntityType.GOOMBA || entity.type === EntityType.MUSHROOM) entity.direction *= -1;
        if (entity.type === EntityType.FIREBALL) { entity.dead = true; spawnParticles(entity.pos.x, entity.pos.y, COLORS.FIRE_RED); }
      }
    }
  };

  const resolveYCollision = (entity: Entity, map: number[][]) => {
    const startX = Math.floor(entity.pos.x / TILE_SIZE);
    const endX = Math.floor((entity.pos.x + entity.width - 0.1) / TILE_SIZE);

    if (entity.vel.y > 0) { // Falling
      const bottomY = Math.floor((entity.pos.y + entity.height) / TILE_SIZE);
      if (isBlocked(startX, bottomY, map) || isBlocked(endX, bottomY, map)) {
        entity.pos.y = bottomY * TILE_SIZE - entity.height;
        if (entity.type === EntityType.FIREBALL) {
           entity.vel.y = -3; // Bounce force (reduced)
        } else {
           entity.vel.y = 0;
           entity.grounded = true;
        }
      }
    } else if (entity.vel.y < 0) { // Jumping up
      const topY = Math.floor(entity.pos.y / TILE_SIZE);
      if (isBlocked(startX, topY, map) || isBlocked(endX, topY, map)) {
        entity.pos.y = (topY + 1) * TILE_SIZE;
        entity.vel.y = 0;
        
        if (entity.type === EntityType.FIREBALL) {
            entity.dead = true;
            spawnParticles(entity.pos.x, entity.pos.y, COLORS.FIRE_RED);
        }

        if (entity.type === EntityType.PLAYER) {
          audioService.playBump();
          handleBlockHit(startX, topY, map);
          if (startX !== endX) handleBlockHit(endX, topY, map);
        }
      }
    }
  };

  const isBlocked = (x: number, y: number, map: number[][]) => {
    if (!map[y] || map[y][x] === undefined) return false;
    return isSolid(map[y][x]);
  };

  const handleBlockHit = (x: number, y: number, map: number[][]) => {
    if (!map[y]) return;
    const tile = map[y][x];
    const player = gameState.current.player;

    if (tile === TileType.BRICK) {
      if (player.powerMode === 'big' || player.powerMode === 'fire') {
        map[y][x] = TileType.AIR;
        spawnParticles(x * TILE_SIZE + 8, y * TILE_SIZE + 8, COLORS.BRICK);
        audioService.playBreak();
        onScore(50);
      } else {
        audioService.playBump();
      }
    } else if (tile === TileType.QUESTION_BLOCK) {
      map[y][x] = TileType.QUESTION_BLOCK_HIT;
      
      const isPowerUp = (x === 21 || x === 78 || x === 109); 
      
      if (isPowerUp) {
         audioService.playPowerUpAppear();
         const type = player.powerMode === 'small' ? EntityType.MUSHROOM : EntityType.FLOWER;
         gameState.current.entities.push({
            id: Date.now(),
            type: type,
            pos: { x: x * TILE_SIZE, y: y * TILE_SIZE }, // Start inside block
            vel: { x: 0, y: -0.3 }, // Pop up slowly (reduced)
            width: 16, height: 16,
            dead: false, grounded: false, direction: 1,
            state: 'spawning',
            frameTimer: 45 // Spawning duration
         });
      } else {
        audioService.playCoin();
        onCoin();
        onScore(200);
        gameState.current.particles.push({
            x: x * TILE_SIZE + 4, y: y * TILE_SIZE - 16, vy: -3, type: 'coin_popup', life: 20
        });
      }
    }
  };

  const spawnParticles = (x: number, y: number, color: string) => {
    for(let i=0; i<4; i++) {
      gameState.current.particles.push({
        x, y, vx: (Math.random() - 0.5) * 3, vy: -1.5 - Math.random() * 3, color, life: 60, type: 'brick'
      });
    }
  };

  const triggerFlagSequence = () => {
    if (cutsceneState.current !== 'none' || !flagEntityRef.current) return;
    cutsceneState.current = 'sliding';
    audioService.playFlagSlide();
    gameState.current.player.vel = { x: 0, y: 1.5 }; // Slide down (reduced)
    // Snap to pole position (flag is offset, pole is at flag.x - 6)
    gameState.current.player.pos.x = flagEntityRef.current.pos.x - 6 + 2;
  };

  const update = () => {
    if (status !== GameStatus.PLAYING) return;

    const player = gameState.current.player;
    const map = gameState.current.map;
    const isCutscene = cutsceneState.current !== 'none';

    if (player.invulnerable > 0) player.invulnerable--;

    // --- Cutscene Logic ---
    if (isCutscene) {
       if (cutsceneState.current === 'sliding') {
          if (flagEntityRef.current && flagEntityRef.current.pos.y < 11 * TILE_SIZE) {
              flagEntityRef.current.pos.y += 1.5;
          }
          if (player.pos.y < 11 * TILE_SIZE) {
              player.pos.y += 1.5;
          } else {
              cutsceneState.current = 'walking_castle';
              player.pos.x += 8;
              player.direction = 1;
              setTimeout(() => audioService.playVictory(), 500);
          }
       } else if (cutsceneState.current === 'walking_castle') {
          player.pos.x += 1.0; // Walk to castle slower
          if (player.pos.x > (202 * TILE_SIZE + 8)) {
             if (player.pos.x > 204 * TILE_SIZE) onWin();
          }
       }
       frameCountRef.current++;
       return;
    }

    // --- Player Physics ---
    if (!player.dead) {
      const targetSpeed = keys.current.run ? MAX_RUN_SPEED : MAX_WALK_SPEED;

      // Input
      if (keys.current.left) {
        player.vel.x -= ACCELERATION;
        if (player.vel.x < -targetSpeed) player.vel.x = -targetSpeed;
        player.direction = -1;
      } else if (keys.current.right) {
        player.vel.x += ACCELERATION;
        if (player.vel.x > targetSpeed) player.vel.x = targetSpeed;
        player.direction = 1;
      } else {
        player.vel.x *= FRICTION;
        if (Math.abs(player.vel.x) < 0.1) player.vel.x = 0;
      }

      player.vel.y += GRAVITY;
      player.pos.x += player.vel.x;
      resolveXCollision(player, map);
      player.pos.y += player.vel.y;
      player.grounded = false; 
      resolveYCollision(player, map);

      // Check Flag Collision - can touch flag by jumping on it or touching pole
      if (flagEntityRef.current) {
        const flag = flagEntityRef.current;
        // Check if player collides with flag entity (jumping on flag)
        if (checkCollision(player, flag)) {
          triggerFlagSequence();
        }
        // Also check if player touches the pole area
        const poleX = flag.pos.x - 6; // Pole is 4 pixels wide, flag is offset
        if (player.pos.x + player.width > poleX && player.pos.x < poleX + 4 && 
            player.pos.y + player.height > flag.pos.y) {
          triggerFlagSequence();
        }
      }

      if (player.pos.y > SCREEN_HEIGHT + 32) {
        die();
      }

      // Camera
      const cameraTarget = player.pos.x - SCREEN_WIDTH / 2 + player.width / 2;
      if (cameraTarget > gameState.current.camera.x) {
          gameState.current.camera.x = cameraTarget;
      }
      if (player.pos.x < gameState.current.camera.x) {
        player.pos.x = gameState.current.camera.x;
        player.vel.x = 0;
      }
    } else {
      player.pos.y += player.vel.y;
      player.vel.y += GRAVITY;
    }

    // --- Entities ---
    gameState.current.entities.forEach(entity => {
      if (entity.dead) return;

      if (entity.state === 'spawning') {
         if (entity.frameTimer && entity.frameTimer > 0) {
             entity.frameTimer--;
             entity.pos.y -= 0.3; 
             if (entity.frameTimer <= 0) {
                 entity.state = 'active';
                 if (entity.type === EntityType.MUSHROOM) entity.vel.x = 0.8; // Slower mushroom 
             }
         }
         return;
      }
      
      if (entity.pos.x > gameState.current.camera.x - 64 && entity.pos.x < gameState.current.camera.x + SCREEN_WIDTH + 64) {
          
          if (entity.type !== EntityType.FLOWER) { 
             entity.vel.y += GRAVITY;
             if (entity.type === EntityType.MUSHROOM || entity.type === EntityType.GOOMBA) {
                // Reduce entity move speeds
                entity.vel.x = entity.direction * (entity.type === EntityType.MUSHROOM ? 0.8 : 0.4);
             }
             
             entity.pos.x += entity.vel.x;
             resolveXCollision(entity, map);
             
             entity.pos.y += entity.vel.y;
             entity.grounded = false;
             resolveYCollision(entity, map);
          }

          // Fireball vs Enemies
          if (entity.type === EntityType.FIREBALL && !entity.dead) {
             gameState.current.entities.forEach(target => {
                if (target.type === EntityType.GOOMBA && !target.dead) {
                    if (checkCollision(entity, target)) {
                        entity.dead = true;
                        target.dead = true;
                        audioService.playKick();
                        onScore(100);
                        spawnParticles(target.pos.x, target.pos.y, COLORS.GOOMBA);
                        spawnParticles(entity.pos.x, entity.pos.y, COLORS.FIRE_RED);
                    }
                }
             });
          }

          // Player Interactions
          if (!player.dead && checkCollision(player, entity)) {
             if (entity.type === EntityType.GOOMBA) {
                if (player.vel.y > 0 && player.pos.y + player.height < entity.pos.y + entity.height / 2 + 6) {
                   entity.dead = true;
                   player.vel.y = -BOUNCE_FORCE; 
                   audioService.playStomp();
                   onScore(100);
                   spawnParticles(entity.pos.x, entity.pos.y, COLORS.GOOMBA);
                } else if (player.invulnerable <= 0) {
                   damagePlayer();
                }
             } else if (entity.type === EntityType.MUSHROOM) {
                entity.dead = true;
                audioService.playPowerUp();
                onScore(1000);
                if (player.powerMode === 'small') {
                    player.powerMode = 'big';
                    player.height = 32;
                    player.pos.y -= 16; 
                }
             } else if (entity.type === EntityType.FLOWER) {
                entity.dead = true;
                audioService.playPowerUp();
                onScore(1000);
                if (player.powerMode === 'small') {
                    player.height = 32;
                    player.pos.y -= 16;
                }
                player.powerMode = 'fire';
             }
          }
          
          if (entity.pos.y > SCREEN_HEIGHT + 32) entity.dead = true;
      }
    });

    // --- Particles ---
    for (let i = gameState.current.particles.length - 1; i >= 0; i--) {
      const p = gameState.current.particles[i];
      if (p.type === 'brick') {
        p.x += p.vx;
        p.y += p.vy;
        p.vy += GRAVITY;
      } else if (p.type === 'coin_popup') {
        p.y += p.vy;
        p.vy += GRAVITY;
      }
      p.life--;
      if (p.life <= 0) gameState.current.particles.splice(i, 1);
    }

    frameCountRef.current++;
  };

  const damagePlayer = () => {
      const player = gameState.current.player;
      if (player.powerMode === 'small') {
          die();
      } else {
          player.powerMode = 'small';
          player.height = 16;
          player.invulnerable = 120; 
          audioService.playBump(); 
      }
  };

  const die = () => {
    if (gameState.current.player.dead) return;
    gameState.current.player.dead = true;
    gameState.current.player.vel.y = -3; // Reduced death jump
    audioService.playDie();
    onDie();
  };

  const draw = (ctx: CanvasRenderingContext2D) => {
    ctx.fillStyle = gameState.current.player.powerMode === 'fire' && frameCountRef.current % 4 < 2 ? '#000' : COLORS.SKY; 
    ctx.fillStyle = COLORS.SKY;
    ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);

    ctx.save();
    ctx.scale(SCALE, SCALE);
    
    const camX = Math.floor(gameState.current.camera.x);
    ctx.translate(-camX, 0);

    const startCol = Math.floor(camX / TILE_SIZE);
    const endCol = startCol + (SCREEN_WIDTH / TILE_SIZE) + 1;
    const map = gameState.current.map;

    for (let y = 0; y < map.length; y++) {
      for (let x = startCol; x <= endCol; x++) {
        if (!map[y] || !map[y][x]) continue;
        const tile = map[y][x];
        const tx = x * TILE_SIZE;
        const ty = y * TILE_SIZE;

        if (tile === TileType.GROUND) {
           ctx.fillStyle = COLORS.GROUND;
           ctx.fillRect(tx, ty, TILE_SIZE, TILE_SIZE);
           ctx.fillStyle = "#903000";
           ctx.fillRect(tx+2, ty+2, 4, 4);
        } else if (tile === TileType.BRICK) {
           ctx.fillStyle = COLORS.BRICK;
           ctx.fillRect(tx, ty, TILE_SIZE, TILE_SIZE);
           ctx.fillStyle = "#000";
           ctx.fillRect(tx, ty, 16, 1);
           ctx.fillRect(tx, ty+8, 16, 1);
           ctx.fillRect(tx+8, ty, 1, 8);
           ctx.fillRect(tx, ty+8, 1, 8);
        } else if (tile === TileType.QUESTION_BLOCK) {
           ctx.fillStyle = COLORS.QUESTION;
           ctx.fillRect(tx, ty, TILE_SIZE, TILE_SIZE);
           ctx.fillStyle = "#000"; 
           ctx.fillText("?", tx + 4, ty + 12);
           ctx.fillRect(tx, ty, 2, 2);
           ctx.fillRect(tx+14, ty, 2, 2);
           ctx.fillRect(tx, ty+14, 2, 2);
           ctx.fillRect(tx+14, ty+14, 2, 2);
        } else if (tile === TileType.QUESTION_BLOCK_HIT) {
           ctx.fillStyle = "#9c6848";
           ctx.fillRect(tx, ty, TILE_SIZE, TILE_SIZE);
           ctx.fillStyle = "#000";
           ctx.fillRect(tx, ty, 1, 16);
           ctx.fillRect(tx+15, ty, 1, 16);
           ctx.fillRect(tx, ty, 16, 1);
           ctx.fillRect(tx, ty+15, 16, 1);
           ctx.fillRect(tx+2, ty+2, 2, 2);
           ctx.fillRect(tx+12, ty+2, 2, 2);
           ctx.fillRect(tx+2, ty+12, 2, 2);
           ctx.fillRect(tx+12, ty+12, 2, 2);
        } else if (tile === TileType.HARD_BLOCK) {
           ctx.fillStyle = COLORS.BRICK;
           ctx.fillRect(tx, ty, TILE_SIZE, TILE_SIZE);
           ctx.fillStyle = "#00000040";
           ctx.fillRect(tx, ty, TILE_SIZE, TILE_SIZE);
           ctx.strokeStyle = "black";
           ctx.strokeRect(tx, ty, TILE_SIZE, TILE_SIZE);
        } else if (tile === TileType.PIPE_L || tile === TileType.PIPE_R || tile === TileType.PIPE_TOP_L || tile === TileType.PIPE_TOP_R) {
           ctx.fillStyle = COLORS.PIPE;
           ctx.fillRect(tx, ty, TILE_SIZE, TILE_SIZE);
           ctx.strokeStyle = COLORS.PIPE_DARK;
           ctx.lineWidth = 2;
           if (tile === TileType.PIPE_TOP_L || tile === TileType.PIPE_TOP_R) {
             ctx.strokeRect(tx, ty, TILE_SIZE, TILE_SIZE);
           } else {
             ctx.beginPath();
             ctx.moveTo(tx, ty); ctx.lineTo(tx, ty+16);
             ctx.moveTo(tx+16, ty); ctx.lineTo(tx+16, ty+16);
             ctx.stroke();
           }
        } else if (tile === TileType.POLE) {
           ctx.fillStyle = "#208020";
           ctx.fillRect(tx + 6, ty, 4, 16);
        } else if (tile === TileType.CLOUD) {
           ctx.fillStyle = COLORS.CLOUD;
           ctx.beginPath();
           ctx.arc(tx + 8, ty + 8, 12, 0, Math.PI * 2);
           ctx.arc(tx - 2, ty + 10, 10, 0, Math.PI * 2);
           ctx.arc(tx + 18, ty + 10, 10, 0, Math.PI * 2);
           ctx.fill();
        } else if (tile === TileType.BUSH) {
           ctx.fillStyle = COLORS.BUSH;
           ctx.beginPath();
           ctx.arc(tx + 8, ty + 12, 8, 0, Math.PI * 2);
           ctx.arc(tx - 2, ty + 12, 6, 0, Math.PI * 2);
           ctx.arc(tx + 18, ty + 12, 6, 0, Math.PI * 2);
           ctx.fill();
        } else if (tile === TileType.HILL) {
            ctx.fillStyle = COLORS.HILL;
            ctx.beginPath();
            ctx.moveTo(tx + 8, ty);
            ctx.lineTo(tx + 24, ty + 16);
            ctx.lineTo(tx - 8, ty + 16);
            ctx.fill();
            ctx.strokeStyle = COLORS.HILL_OUTLINE;
            ctx.lineWidth = 1;
            ctx.stroke();
        } else if (tile === TileType.CASTLE) {
            ctx.fillStyle = COLORS.BRICK;
            ctx.fillRect(tx - 16, ty - 16, 48, 32); 
            ctx.fillStyle = "#000";
            ctx.fillRect(tx + 4, ty + 8, 8, 8); 
        }
      }
    }

    // Draw Entities
    if (flagEntityRef.current) {
       const f = flagEntityRef.current;
       ctx.fillStyle = "#208020"; 
       ctx.fillRect(f.pos.x, f.pos.y, 4, 16);
       ctx.fillStyle = COLORS.MARIO_RED;
       ctx.beginPath();
       ctx.moveTo(f.pos.x+4, f.pos.y);
       ctx.lineTo(f.pos.x-12, f.pos.y+4);
       ctx.lineTo(f.pos.x+4, f.pos.y+8);
       ctx.fill();
    }

    gameState.current.entities.forEach(e => {
       if (e.dead) return;
       if (e.type === EntityType.GOOMBA) {
          drawSprite(ctx, GOOMBA_SPRITE, e.pos.x, e.pos.y, e.direction === -1, COLORS.GOOMBA, COLORS.BLACK, COLORS.WHITE);
       } else if (e.type === EntityType.MUSHROOM) {
          if (e.state === 'spawning') ctx.globalCompositeOperation = 'destination-over';
          drawSprite(ctx, MUSHROOM_SPRITE, e.pos.x, e.pos.y, false, COLORS.MUSHROOM_RED, COLORS.MUSHROOM_SKIN, COLORS.BLACK);
          if (e.state === 'spawning') ctx.globalCompositeOperation = 'source-over';
       } else if (e.type === EntityType.FLOWER) {
          if (e.state === 'spawning') ctx.globalCompositeOperation = 'destination-over';
          const c1 = Math.floor(Date.now() / 100) % 2 === 0 ? COLORS.FIRE_RED : COLORS.WHITE;
          const c2 = Math.floor(Date.now() / 100) % 2 === 0 ? COLORS.MUSHROOM_SKIN : COLORS.FIRE_RED;
          drawSprite(ctx, FLOWER_SPRITE, e.pos.x, e.pos.y, false, c1, c2, COLORS.HILL);
          if (e.state === 'spawning') ctx.globalCompositeOperation = 'source-over';
       } else if (e.type === EntityType.FIREBALL) {
          // Rotate fireball by flipping every few frames
          const flip = Math.floor(frameCountRef.current / 4) % 2 === 0;
          drawSprite(ctx, FIREBALL_SPRITE, e.pos.x, e.pos.y, flip, COLORS.FIREBALL_CENTER, COLORS.FIREBALL_OUTER, COLORS.WHITE);
       }
    });

    const p = gameState.current.player;
    
    // Invulnerability Blinking
    if (p.invulnerable > 0 && Math.floor(frameCountRef.current / 4) % 2 === 0) {
       // Don't draw
    } else {
        const isBig = p.powerMode === 'big' || p.powerMode === 'fire';
        const c1 = p.powerMode === 'fire' ? COLORS.FIRE_WHITE : COLORS.MARIO_RED; 
        const c2 = p.powerMode === 'fire' ? COLORS.MUSHROOM_SKIN : COLORS.MARIO_SKIN; 
        const c3 = p.powerMode === 'fire' ? COLORS.FIRE_RED : COLORS.MARIO_BROWN; 

        if (isBig) {
            drawSprite(ctx, MARIO_SPRITE_BIG, p.pos.x, p.pos.y, p.direction === -1, c1, c2, c3);
        } else {
            drawSprite(ctx, MARIO_SPRITE_STAND, p.pos.x, p.pos.y, p.direction === -1, c1, c2, c3);
        }
    }

    gameState.current.particles.forEach(part => {
      ctx.fillStyle = part.type === 'coin_popup' ? COLORS.QUESTION : part.color;
      if (part.type === 'coin_popup') {
         ctx.fillRect(part.x, part.y, 8, 10);
      } else {
         ctx.fillRect(part.x, part.y, 4, 4);
      }
    });

    ctx.restore();
  };

  const drawSprite = (ctx: CanvasRenderingContext2D, sprite: number[][], x: number, y: number, flip: boolean, c1: string, c2: string, c3: string) => {
    ctx.save();
    ctx.translate(Math.floor(x), Math.floor(y));
    if (flip) {
      ctx.translate(sprite[0].length, 0); // Width of sprite
      ctx.scale(-1, 1);
    }
    for(let r=0; r<sprite.length; r++) {
      for(let c=0; c<sprite[0].length; c++) { 
         const pix = sprite[r][c];
         if(pix === 0) continue;
         ctx.fillStyle = pix === 1 ? c1 : pix === 2 ? c2 : pix === 3 ? c3 : "#000";
         ctx.fillRect(c, r, 1, 1);
      }
    }
    ctx.restore();
  };

  const loop = useCallback((time: number) => {
    update();
    const canvas = canvasRef.current;
    if (canvas) {
      const ctx = canvas.getContext('2d');
      if (ctx) draw(ctx);
    }
    requestRef.current = requestAnimationFrame(loop);
  }, [status]);

  useEffect(() => {
    requestRef.current = requestAnimationFrame(loop);
    return () => cancelAnimationFrame(requestRef.current!);
  }, [loop]);

  return (
    <canvas
      ref={canvasRef}
      width={SCREEN_WIDTH * SCALE}
      height={SCREEN_HEIGHT * SCALE}
      className="bg-black mx-auto border-4 border-gray-700 shadow-2xl rounded-lg cursor-none"
      style={{ width: '100%', maxWidth: `${SCREEN_WIDTH * SCALE}px` }}
    />
  );
};

export default GameCanvas;
