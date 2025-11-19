// Super Blazor Bros - Game Logic
let gameInstance = null;
let dotNetHelper = null;

window.initializeGame = (dotNetRef) => {
    dotNetHelper = dotNetRef;
    console.log('Game initialized');
};

window.startGame = (levelData) => {
    console.log('Starting game with level data:', levelData);
    if (gameInstance) {
        gameInstance.dispose();
    }
    gameInstance = new GameEngine(levelData);
    gameInstance.start();
};

window.sendKeyEvent = (key, isDown) => {
    if (gameInstance) {
        gameInstance.handleKey(key, isDown);
    }
};

class GameEngine {
    constructor(levelData) {
        this.canvas = document.getElementById('gameCanvas');
        this.ctx = this.canvas.getContext('2d');
        this.levelData = levelData;
        
        // Game constants
        this.TILE_SIZE = 16;
        this.SCALE = 3;
        this.SCREEN_WIDTH = 256;
        this.SCREEN_HEIGHT = 240;
        this.GRAVITY = 0.25;
        this.FRICTION = 0.90;
        this.ACCELERATION = 0.12;
        this.MAX_WALK_SPEED = 1.4;
        this.MAX_RUN_SPEED = 2.6;
        this.JUMP_FORCE = 6.6;
        
        // Player state
        this.player = {
            pos: { x: 50, y: 100 },
            vel: { x: 0, y: 0 },
            width: 12,
            height: 16,
            powerMode: 'small',
            isJumping: false,
            grounded: false,
            direction: 1
        };
        
        // Camera
        this.camera = { x: 0 };
        
        // Input
        this.keys = {
            left: false,
            right: false,
            jump: false,
            run: false
        };
        
        // Game state
        this.entities = levelData.entities || [];
        this.particles = [];
        this.frameCount = 0;
        this.running = false;
        
        // Colors
        this.COLORS = {
            SKY: '#5C94FC',
            GROUND: '#C84C0C',
            BRICK: '#B83410',
            QUESTION: '#F8D820',
            PIPE: '#00A800',
            PIPE_DARK: '#006000',
            MARIO_RED: '#F83800',
            MARIO_SKIN: '#FFA044',
            MARIO_BROWN: '#887000',
            BLACK: '#000000',
            WHITE: '#FFFFFF'
        };
        
        // Bind keyboard events
        this.handleKeyDown = this.handleKeyDown.bind(this);
        this.handleKeyUp = this.handleKeyUp.bind(this);
        window.addEventListener('keydown', this.handleKeyDown);
        window.addEventListener('keyup', this.handleKeyUp);
        
        // Find safe starting position
        this.findSafeStartPosition();
    }
    
    findSafeStartPosition() {
        const START_X = 50;
        const START_TILE_X = Math.floor(START_X / this.TILE_SIZE);
        const GROUND_TILES = [1, 2, 3, 5, 6, 7, 8, 9];
        
        for (let offset = 0; offset < 10; offset++) {
            const checkX = Math.max(0, START_TILE_X - offset);
            
            for (let y = 0; y < this.levelData.map.length; y++) {
                if (this.levelData.map[y] && GROUND_TILES.includes(this.levelData.map[y][checkX])) {
                    const groundTopY = y * this.TILE_SIZE;
                    const marioY = groundTopY - this.player.height;
                    
                    if (marioY < groundTopY) {
                        this.player.pos = { x: checkX * this.TILE_SIZE + 2, y: marioY };
                        return;
                    }
                }
            }
        }
        
        // Fallback
        this.player.pos = { x: START_X, y: 12 * this.TILE_SIZE - this.player.height };
    }
    
    handleKeyDown(e) {
        this.updateKeyState(e.code, true);
    }
    
    handleKeyUp(e) {
        this.updateKeyState(e.code, false);
    }
    
    handleKey(key, isDown) {
        this.updateKeyState(key, isDown);
    }
    
    updateKeyState(code, isDown) {
        switch(code) {
            case 'ArrowLeft':
                this.keys.left = isDown;
                break;
            case 'ArrowRight':
                this.keys.right = isDown;
                break;
            case 'Space':
                this.keys.jump = isDown;
                break;
            case 'ShiftLeft':
            case 'ShiftRight':
                this.keys.run = isDown;
                break;
        }
    }
    
    start() {
        this.running = true;
        this.gameLoop();
    }
    
    gameLoop() {
        if (!this.running) return;
        
        this.update();
        this.render();
        
        requestAnimationFrame(() => this.gameLoop());
    }
    
    update() {
        this.frameCount++;
        
        // Player horizontal movement
        if (this.keys.left) {
            this.player.vel.x -= this.ACCELERATION;
            this.player.direction = -1;
        }
        if (this.keys.right) {
            this.player.vel.x += this.ACCELERATION;
            this.player.direction = 1;
        }
        
        // Apply friction
        this.player.vel.x *= this.FRICTION;
        
        // Speed limits
        const maxSpeed = this.keys.run ? this.MAX_RUN_SPEED : this.MAX_WALK_SPEED;
        if (Math.abs(this.player.vel.x) > maxSpeed) {
            this.player.vel.x = maxSpeed * Math.sign(this.player.vel.x);
        }
        
        // Jumping
        if (this.keys.jump && this.player.grounded) {
            this.player.vel.y = -this.JUMP_FORCE;
            this.player.grounded = false;
            this.player.isJumping = true;
        }
        
        // Gravity
        if (!this.player.grounded) {
            this.player.vel.y += this.GRAVITY;
        }
        
        // Update position
        this.player.pos.x += this.player.vel.x;
        this.player.pos.y += this.player.vel.y;
        
        // Simple collision detection
        this.handleCollisions();
        
        // Update camera
        this.updateCamera();
        
        // Update entities
        this.updateEntities();
        
        // Check win condition (reached end of level)
        if (this.player.pos.x > (this.levelData.map[0].length - 10) * this.TILE_SIZE) {
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnWin');
            }
        }
    }
    
    handleCollisions() {
        const GROUND_TILES = [1, 2, 3, 5, 6, 7, 8, 9, 10];
        
        // Ground collision
        this.player.grounded = false;
        const tileX = Math.floor(this.player.pos.x / this.TILE_SIZE);
        const tileY = Math.floor((this.player.pos.y + this.player.height) / this.TILE_SIZE);
        
        if (tileY >= 0 && tileY < this.levelData.map.length) {
            const row = this.levelData.map[tileY];
            if (row && tileX >= 0 && tileX < row.length) {
                if (GROUND_TILES.includes(row[tileX]) || (tileX + 1 < row.length && GROUND_TILES.includes(row[tileX + 1]))) {
                    const groundY = tileY * this.TILE_SIZE;
                    if (this.player.pos.y + this.player.height > groundY && this.player.vel.y >= 0) {
                        this.player.pos.y = groundY - this.player.height;
                        this.player.vel.y = 0;
                        this.player.grounded = true;
                        this.player.isJumping = false;
                    }
                }
            }
        }
        
        // Prevent falling through bottom
        if (this.player.pos.y + this.player.height > this.SCREEN_HEIGHT) {
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnDie');
            }
            this.player.pos = { x: 50, y: 100 };
            this.player.vel = { x: 0, y: 0 };
        }
        
        // Horizontal collision
        const nextX = this.player.pos.x;
        const nextTileX = Math.floor(nextX / this.TILE_SIZE);
        const playerTileY = Math.floor(this.player.pos.y / this.TILE_SIZE);
        
        if (playerTileY >= 0 && playerTileY < this.levelData.map.length) {
            const row = this.levelData.map[playerTileY];
            if (row && nextTileX >= 0 && nextTileX < row.length && GROUND_TILES.includes(row[nextTileX])) {
                if (this.player.vel.x > 0) {
                    this.player.pos.x = nextTileX * this.TILE_SIZE - this.player.width;
                } else if (this.player.vel.x < 0) {
                    this.player.pos.x = (nextTileX + 1) * this.TILE_SIZE;
                }
                this.player.vel.x = 0;
            }
        }
    }
    
    updateCamera() {
        const targetX = this.player.pos.x - this.SCREEN_WIDTH / 3;
        this.camera.x = Math.max(0, targetX);
    }
    
    updateEntities() {
        // Simple enemy movement
        this.entities.forEach(entity => {
            if (entity.dead) return;
            
            entity.pos.x += entity.vel.x;
            
            // Simple collision with player
            if (this.checkCollision(this.player, entity)) {
                if (this.player.vel.y > 0 && this.player.pos.y < entity.pos.y) {
                    // Jump on enemy
                    entity.dead = true;
                    this.player.vel.y = -3;
                    if (dotNetHelper) {
                        dotNetHelper.invokeMethodAsync('OnScore', 100);
                    }
                } else {
                    // Hit by enemy
                    if (dotNetHelper) {
                        dotNetHelper.invokeMethodAsync('OnDie');
                    }
                }
            }
        });
    }
    
    checkCollision(a, b) {
        return a.pos.x < b.pos.x + b.width &&
               a.pos.x + a.width > b.pos.x &&
               a.pos.y < b.pos.y + b.height &&
               a.pos.y + a.height > b.pos.y;
    }
    
    render() {
        // Clear canvas
        this.ctx.fillStyle = this.COLORS.SKY;
        this.ctx.fillRect(0, 0, this.canvas.width, this.canvas.height);
        
        // Render map
        this.renderMap();
        
        // Render entities
        this.renderEntities();
        
        // Render player
        this.renderPlayer();
    }
    
    renderMap() {
        const startX = Math.floor(this.camera.x / this.TILE_SIZE);
        const endX = Math.min(startX + Math.ceil(this.SCREEN_WIDTH / this.TILE_SIZE) + 1, this.levelData.map[0].length);
        
        for (let y = 0; y < this.levelData.map.length; y++) {
            for (let x = startX; x < endX; x++) {
                const tile = this.levelData.map[y][x];
                if (tile === 0) continue;
                
                const screenX = (x * this.TILE_SIZE - this.camera.x) * this.SCALE;
                const screenY = y * this.TILE_SIZE * this.SCALE;
                
                this.renderTile(tile, screenX, screenY);
            }
        }
    }
    
    renderTile(tile, x, y) {
        const size = this.TILE_SIZE * this.SCALE;
        
        switch(tile) {
            case 1: // Ground
                this.ctx.fillStyle = this.COLORS.GROUND;
                this.ctx.fillRect(x, y, size, size);
                break;
            case 2: // Brick
                this.ctx.fillStyle = this.COLORS.BRICK;
                this.ctx.fillRect(x, y, size, size);
                this.ctx.strokeStyle = this.COLORS.BLACK;
                this.ctx.lineWidth = 2;
                this.ctx.strokeRect(x, y, size, size);
                break;
            case 3: // Question Block
                this.ctx.fillStyle = this.COLORS.QUESTION;
                this.ctx.fillRect(x, y, size, size);
                this.ctx.fillStyle = this.COLORS.BLACK;
                this.ctx.font = `${size * 0.6}px Arial`;
                this.ctx.textAlign = 'center';
                this.ctx.textBaseline = 'middle';
                this.ctx.fillText('?', x + size / 2, y + size / 2);
                break;
            case 5: // Hard Block
                this.ctx.fillStyle = '#808080';
                this.ctx.fillRect(x, y, size, size);
                break;
            case 6: case 7: case 8: case 9: // Pipe
                this.ctx.fillStyle = this.COLORS.PIPE;
                this.ctx.fillRect(x, y, size, size);
                break;
            case 10: // Pole
                this.ctx.fillStyle = '#FFD700';
                this.ctx.fillRect(x + size / 2 - 2, y, 4, size);
                break;
            case 11: // Flag
                this.ctx.fillStyle = '#FF0000';
                this.ctx.fillRect(x + size / 2, y, size / 2, size / 2);
                break;
        }
    }
    
    renderPlayer() {
        const screenX = (this.player.pos.x - this.camera.x) * this.SCALE;
        const screenY = this.player.pos.y * this.SCALE;
        const width = this.player.width * this.SCALE;
        const height = this.player.height * this.SCALE;
        
        // Simple Mario sprite
        this.ctx.fillStyle = this.COLORS.MARIO_RED;
        this.ctx.fillRect(screenX, screenY, width, height);
        
        // Cap
        this.ctx.fillStyle = this.COLORS.MARIO_RED;
        this.ctx.fillRect(screenX + 2, screenY, width - 4, height / 4);
        
        // Face
        this.ctx.fillStyle = this.COLORS.MARIO_SKIN;
        this.ctx.fillRect(screenX + 2, screenY + height / 4, width - 4, height / 3);
    }
    
    renderEntities() {
        this.entities.forEach(entity => {
            if (entity.dead) return;
            
            const screenX = (entity.pos.x - this.camera.x) * this.SCALE;
            const screenY = entity.pos.y * this.SCALE;
            const width = entity.width * this.SCALE;
            const height = entity.height * this.SCALE;
            
            // Simple Goomba sprite
            this.ctx.fillStyle = '#C84C0C';
            this.ctx.fillRect(screenX, screenY, width, height);
            
            // Eyes
            this.ctx.fillStyle = this.COLORS.WHITE;
            this.ctx.fillRect(screenX + width * 0.2, screenY + height * 0.3, width * 0.2, height * 0.2);
            this.ctx.fillRect(screenX + width * 0.6, screenY + height * 0.3, width * 0.2, height * 0.2);
        });
    }
    
    dispose() {
        this.running = false;
        window.removeEventListener('keydown', this.handleKeyDown);
        window.removeEventListener('keyup', this.handleKeyUp);
    }
}
