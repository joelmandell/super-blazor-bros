// Game Renderer with Sprite Support - Enhanced WebGL renderer
// Falls back to colored rectangles if sprites are not available

import { getSprite, isSpriteLoaded } from './spriteLoader.js';

let dotNetHelper = null;
let canvas = null;
let ctx = null;
let useSprites = false;

// Game constants
const TILE_SIZE = 16;
const SCALE = 3;
const SCREEN_WIDTH = 256;
const SCREEN_HEIGHT = 240;

export function initialize(dotNetRef) {
    dotNetHelper = dotNetRef;
    canvas = document.getElementById('gameCanvas');
    if (!canvas) {
        console.error('Canvas element not found');
        return;
    }
    
    // Use 2D context for sprite rendering
    ctx = canvas.getContext('2d');
    if (!ctx) {
        console.error('2D context not available');
        return;
    }
    
    console.log('Renderer with sprite support initialized');
    
    // Disable image smoothing for pixel-perfect rendering
    ctx.imageSmoothingEnabled = false;
    ctx.webkitImageSmoothingEnabled = false;
    ctx.mozImageSmoothingEnabled = false;
    
    // Setup keyboard input
    window.addEventListener('keydown', (e) => {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnKeyDown', e.code);
            if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Space'].includes(e.code)) {
                e.preventDefault();
            }
        }
    });
    
    window.addEventListener('keyup', (e) => {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnKeyUp', e.code);
        }
    });
}

export function setUseSpriteMode(enabled) {
    useSprites = enabled;
    console.log(`Sprite mode: ${enabled ? 'enabled' : 'disabled (using colored rectangles)'}`);
}

export function render(renderData) {
    if (!ctx) return;
    
    // Clear canvas with sky color
    ctx.fillStyle = '#5C94FC';
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    // Save context state
    ctx.save();
    
    // Apply camera transformation
    const cameraX = (renderData.cameraX || 0) * SCALE;
    ctx.translate(-cameraX, 0);
    
    // Render level
    if (renderData.level) {
        renderLevel(renderData.level);
    }
    
    // Render entities
    if (renderData.entities) {
        renderData.entities.forEach(entity => renderEntity(entity));
    }
    
    // Render player
    if (renderData.player) {
        renderPlayer(renderData.player);
    }
    
    // Restore context state
    ctx.restore();
}

function renderLevel(map) {
    if (!map) return;
    
    const tileColors = {
        1: '#C84C0C', // Ground
        2: '#B83410', // Brick
        3: '#F8D820', // Question block
        4: '#A0A0A0', // Used question block
        5: '#808080', // Hard block
        6: '#00A800', // Pipe
        7: '#00A800', // Pipe
        8: '#00A800', // Pipe top left
        9: '#00A800', // Pipe top right
        12: '#F8D820' // Coin
    };
    
    const tileSprites = {
        1: 'ground',
        2: 'brick',
        3: 'question-block',
        4: 'question-block-used',
        5: 'platform',
        6: 'pipe-top-left',
        7: 'pipe-top-right',
        8: 'pipe-vertical',
        9: 'pipe-vertical',
        12: 'coin'
    };
    
    for (let y = 0; y < map.length; y++) {
        if (!map[y]) continue;
        
        for (let x = 0; x < map[y].length; x++) {
            const tile = map[y][x];
            if (tile === 0) continue;
            
            const tileX = x * TILE_SIZE * SCALE;
            const tileY = y * TILE_SIZE * SCALE;
            const tileW = TILE_SIZE * SCALE;
            const tileH = TILE_SIZE * SCALE;
            
            // Try to render sprite, fall back to colored rectangle
            if (useSprites && tileSprites[tile]) {
                const sprite = getSprite(tileSprites[tile]);
                if (sprite) {
                    ctx.drawImage(sprite, tileX, tileY, tileW, tileH);
                    continue;
                }
            }
            
            // Fallback to colored rectangles
            ctx.fillStyle = tileColors[tile] || '#FFFFFF';
            ctx.fillRect(tileX, tileY, tileW, tileH);
            
            // Add details for specific tiles
            if (tile === 3) {
                // Question mark on question block
                ctx.fillStyle = '#000000';
                const centerX = tileX + tileW / 2;
                const centerY = tileY + tileH / 2;
                ctx.fillRect(centerX - 4, centerY - 8, 8, 16);
            } else if (tile === 12) {
                // Coin shine effect
                const coinSize = tileW * 0.5;
                const coinX = tileX + (tileW - coinSize) / 2;
                const coinY = tileY + (tileH - coinSize) / 2;
                ctx.fillStyle = '#F8D820';
                ctx.fillRect(coinX, coinY, coinSize, coinSize);
                ctx.fillStyle = '#FFFFFF';
                const shineSize = coinSize * 0.3;
                ctx.fillRect(coinX + shineSize * 0.5, coinY + shineSize * 0.5, shineSize, shineSize);
            }
        }
    }
}

function renderPlayer(player) {
    const x = Math.floor(player.pos.x * SCALE);
    const y = Math.floor(player.pos.y * SCALE);
    const w = player.width * SCALE;
    const h = player.height * SCALE;
    
    // Try to render sprite
    if (useSprites) {
        const spriteName = getPlayerSpriteName(player.powerMode || 'small');
        const sprite = getSprite(spriteName);
        if (sprite) {
            // Flip sprite based on direction
            if (player.direction < 0) {
                ctx.save();
                ctx.scale(-1, 1);
                ctx.drawImage(sprite, -x - w, y, w, h);
                ctx.restore();
            } else {
                ctx.drawImage(sprite, x, y, w, h);
            }
            return;
        }
    }
    
    // Fallback to colored rectangles (Mario)
    const red = '#F83800';
    const skin = '#FFA044';
    const brown = '#887000';
    
    // Hat
    ctx.fillStyle = red;
    ctx.fillRect(x + 2, y, w - 4, 4);
    
    // Face
    ctx.fillStyle = skin;
    ctx.fillRect(x + 2, y + 4, w - 4, 6);
    
    // Body
    ctx.fillStyle = red;
    ctx.fillRect(x + 2, y + 10, w - 4, 4);
    
    // Legs
    ctx.fillStyle = brown;
    ctx.fillRect(x + 2, y + 14, 4, 2);
    ctx.fillRect(x + w - 6, y + 14, 4, 2);
}

function renderEntity(entity) {
    const x = Math.floor(entity.pos.x * SCALE);
    const y = Math.floor(entity.pos.y * SCALE);
    const w = (entity.width || 16) * SCALE;
    const h = (entity.height || 16) * SCALE;
    
    // Try to render sprite
    if (useSprites) {
        const spriteName = getEntitySpriteName(entity.type, entity.state);
        const sprite = getSprite(spriteName);
        if (sprite) {
            ctx.drawImage(sprite, x, y, w, h);
            return;
        }
    }
    
    // Fallback to colored shapes
    // EntityType: MUSHROOM = 3, GOOMBA = 1, KOOPA = 2
    if (entity.type === 3 || entity.state === 'mushroom') {
        // Mushroom
        ctx.fillStyle = '#F81018';
        ctx.fillRect(x, y, w, h * 0.6);
        
        ctx.fillStyle = '#FFFFFF';
        const spotSize = w * 0.25;
        ctx.fillRect(x + w * 0.2, y + h * 0.15, spotSize, spotSize * 0.8);
        ctx.fillRect(x + w * 0.55, y + h * 0.15, spotSize, spotSize * 0.8);
        
        ctx.fillStyle = '#FFF8DC';
        ctx.fillRect(x + w * 0.3, y + h * 0.6, w * 0.4, h * 0.4);
    } else if (entity.type === 1) {
        // Goomba (brown enemy)
        ctx.fillStyle = '#A06020';
        ctx.fillRect(x, y, w, h);
        
        ctx.fillStyle = '#000000';
        ctx.fillRect(x + w * 0.2, y + h * 0.2, w * 0.2, h * 0.2);
        ctx.fillRect(x + w * 0.6, y + h * 0.2, w * 0.2, h * 0.2);
    } else if (entity.type === 2) {
        // Koopa (green enemy)
        ctx.fillStyle = '#00A800';
        ctx.fillRect(x, y, w, h * 0.7);
        
        ctx.fillStyle = '#F8D820';
        ctx.fillRect(x, y + h * 0.7, w, h * 0.3);
        
        ctx.fillStyle = '#000000';
        ctx.fillRect(x + w * 0.2, y + h * 0.2, w * 0.15, h * 0.15);
        ctx.fillRect(x + w * 0.65, y + h * 0.2, w * 0.15, h * 0.15);
    }
}

function getPlayerSpriteName(powerMode) {
    switch (powerMode.toLowerCase()) {
        case 'big':
            return 'mario-big';
        case 'fire':
            return 'mario-fire';
        case 'small':
        default:
            return 'mario-small';
    }
}

function getEntitySpriteName(type, state) {
    // EntityType: MUSHROOM = 3, GOOMBA = 1, KOOPA = 2
    if (type === 3 || state === 'mushroom') {
        return 'mushroom';
    } else if (type === 1) {
        return 'goomba';
    } else if (type === 2) {
        if (state === 'shell') {
            return 'shell-green';
        }
        return 'koopa-green';
    }
    return null;
}

export function playSound(soundName) {
    // Placeholder for sound functionality
    console.log(`Play sound: ${soundName}`);
}
