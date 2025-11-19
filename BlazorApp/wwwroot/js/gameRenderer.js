// Game Renderer - Minimal JavaScript for Canvas rendering
// All game logic runs in C# WebAssembly

let dotNetHelper = null;
let canvas = null;
let ctx = null;

// Game constants
const TILE_SIZE = 16;
const SCALE = 3;
const SCREEN_WIDTH = 256;
const SCREEN_HEIGHT = 240;

// Colors
const COLORS = {
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
    WHITE: '#FFFFFF',
    COIN_YELLOW: '#FFD700'
};

export function initialize(dotNetRef) {
    dotNetHelper = dotNetRef;
    canvas = document.getElementById('gameCanvas');
    if (canvas) {
        ctx = canvas.getContext('2d');
        ctx.imageSmoothingEnabled = false;
    }
    
    // Setup keyboard input
    window.addEventListener('keydown', (e) => {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnKeyDown', e.code);
            // Prevent default for game keys
            if (['ArrowUp', 'ArrowDown', 'Arrow Left', 'ArrowRight', 'Space'].includes(e.code)) {
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

export function startRendering(levelData) {
    console.log('Rendering started with level data');
}

export function render(renderData) {
    if (!ctx || !canvas) return;
    
    const { player, entities, cameraX, level } = renderData;
    
    // Clear canvas
    ctx.fillStyle = COLORS.SKY;
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    // Save context for camera transform
    ctx.save();
    ctx.scale(SCALE, SCALE);
    ctx.translate(-Math.floor(cameraX), 0);
    
    // Render level
    renderLevel(level);
    
    // Render entities
    if (entities && entities.length > 0) {
        entities.forEach(entity => renderEntity(entity));
    }
    
    // Render player
    if (player) {
        renderPlayer(player);
    }
    
    // Restore context
    ctx.restore();
}

function renderLevel(map) {
    if (!map) return;
    
    for (let y = 0; y < map.length; y++) {
        if (!map[y]) continue;
        
        for (let x = 0; x < map[y].length; x++) {
            const tile = map[y][x];
            const tileX = x * TILE_SIZE;
            const tileY = y * TILE_SIZE;
            
            switch (tile) {
                case 1: // Ground
                    ctx.fillStyle = COLORS.GROUND;
                    ctx.fillRect(tileX, tileY, TILE_SIZE, TILE_SIZE);
                    ctx.fillStyle = COLORS.BLACK;
                    ctx.fillRect(tileX, tileY, TILE_SIZE, 1);
                    ctx.fillRect(tileX, tileY, 1, TILE_SIZE);
                    break;
                    
                case 2: // Brick
                    ctx.fillStyle = COLORS.BRICK;
                    ctx.fillRect(tileX, tileY, TILE_SIZE, TILE_SIZE);
                    ctx.fillStyle = COLORS.BLACK;
                    for (let i = 0; i < 4; i++) {
                        ctx.fillRect(tileX, tileY + i * 4, TILE_SIZE, 1);
                        ctx.fillRect(tileX + i * 4, tileY, 1, TILE_SIZE);
                    }
                    break;
                    
                case 3: // Question block
                    ctx.fillStyle = COLORS.QUESTION;
                    ctx.fillRect(tileX, tileY, TILE_SIZE, TILE_SIZE);
                    ctx.fillStyle = COLORS.BLACK;
                    ctx.fillRect(tileX + 6, tileY + 4, 4, 8);
                    ctx.fillRect(tileX + 7, tileY + 3, 2, 2);
                    break;
                    
                case 4: // Coin
                    ctx.fillStyle = COLORS.COIN_YELLOW;
                    ctx.beginPath();
                    ctx.arc(tileX + TILE_SIZE / 2, tileY + TILE_SIZE / 2, 4, 0, Math.PI * 2);
                    ctx.fill();
                    break;
                    
                case 5: // Pipe (top left)
                case 6: // Pipe (top right)
                case 7: // Pipe (bottom left)
                case 8: // Pipe (bottom right)
                    ctx.fillStyle = COLORS.PIPE;
                    ctx.fillRect(tileX, tileY, TILE_SIZE, TILE_SIZE);
                    ctx.fillStyle = COLORS.PIPE_DARK;
                    if (tile === 5 || tile === 7) {
                        ctx.fillRect(tileX, tileY, 2, TILE_SIZE);
                    } else {
                        ctx.fillRect(tileX + TILE_SIZE - 2, tileY, 2, TILE_SIZE);
                    }
                    break;
            }
        }
    }
}

function renderPlayer(player) {
    const x = Math.floor(player.pos.x);
    const y = Math.floor(player.pos.y);
    const w = player.width;
    const h = player.height;
    
    // Simple Mario sprite (red and skin color)
    // Hat
    ctx.fillStyle = COLORS.MARIO_RED;
    ctx.fillRect(x + 2, y, w - 4, 4);
    
    // Face
    ctx.fillStyle = COLORS.MARIO_SKIN;
    ctx.fillRect(x + 2, y + 4, w - 4, 6);
    
    // Body
    ctx.fillStyle = COLORS.MARIO_RED;
    ctx.fillRect(x + 2, y + 10, w - 4, 4);
    
    // Legs
    ctx.fillStyle = COLORS.MARIO_BROWN;
    ctx.fillRect(x + 2, y + 14, 4, 2);
    ctx.fillRect(x + w - 6, y + 14, 4, 2);
    
    // Outline
    ctx.strokeStyle = COLORS.BLACK;
    ctx.lineWidth = 0.5;
    ctx.strokeRect(x, y, w, h);
}

function renderEntity(entity) {
    const x = Math.floor(entity.pos.x);
    const y = Math.floor(entity.pos.y);
    const w = entity.width || 16;
    const h = entity.height || 16;
    
    // EntityType.GOOMBA = 1
    if (entity.type === 1) {
        // Goomba (brown mushroom enemy)
        ctx.fillStyle = COLORS.MARIO_BROWN;
        ctx.fillRect(x, y, w, h);
        
        // Eyes
        ctx.fillStyle = COLORS.WHITE;
        ctx.fillRect(x + 3, y + 4, 4, 4);
        ctx.fillRect(x + w - 7, y + 4, 4, 4);
        
        ctx.fillStyle = COLORS.BLACK;
        ctx.fillRect(x + 4, y + 5, 2, 2);
        ctx.fillRect(x + w - 6, y + 5, 2, 2);
    }
}

// Handle touch/mouse controls
export function sendKeyEvent(key, isDown) {
    if (!dotNetHelper) return;
    
    if (isDown) {
        dotNetHelper.invokeMethodAsync('OnKeyDown', key);
    } else {
        dotNetHelper.invokeMethodAsync('OnKeyUp', key);
    }
}
