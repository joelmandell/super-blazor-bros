// Game Renderer - WebGL version for hardware-accelerated 2D rendering
// All game logic runs in C# WebAssembly

let dotNetHelper = null;
let canvas = null;
let gl = null;
let shaderProgram = null;
let vertexBuffer = null;
let texCoordBuffer = null;
let indexBuffer = null;

// Game constants
const TILE_SIZE = 16;
const SCALE = 3;
const SCREEN_WIDTH = 256;
const SCREEN_HEIGHT = 240;

// Vertex shader source - transforms 2D coordinates
const vertexShaderSource = `
    attribute vec2 aPosition;
    attribute vec2 aTexCoord;
    
    uniform vec2 uResolution;
    uniform vec2 uTranslation;
    
    varying vec2 vTexCoord;
    
    void main() {
        // Convert from pixel coordinates to clip space (-1 to 1)
        vec2 position = (aPosition + uTranslation) / uResolution * 2.0 - 1.0;
        position.y = -position.y; // Flip Y axis (WebGL has origin at bottom-left)
        
        gl_Position = vec4(position, 0.0, 1.0);
        vTexCoord = aTexCoord;
    }
`;

// Fragment shader source - applies colors
const fragmentShaderSource = `
    precision mediump float;
    
    uniform vec4 uColor;
    varying vec2 vTexCoord;
    
    void main() {
        gl_FragColor = uColor;
    }
`;

export function initialize(dotNetRef) {
    dotNetHelper = dotNetRef;
    canvas = document.getElementById('gameCanvas');
    if (!canvas) {
        console.error('Canvas element not found');
        return;
    }
    
    // Get WebGL context
    gl = canvas.getContext('webgl') || canvas.getContext('experimental-webgl');
    if (!gl) {
        console.error('WebGL not supported, falling back to 2D canvas');
        // Could fall back to 2D canvas here
        return;
    }
    
    console.log('WebGL initialized successfully');
    
    // Initialize shaders and buffers
    initializeShaders();
    initializeBuffers();
    
    // Setup WebGL state
    gl.viewport(0, 0, canvas.width, canvas.height);
    gl.clearColor(0.36, 0.58, 0.99, 1.0); // Sky blue #5C94FC
    
    // Setup keyboard input
    window.addEventListener('keydown', (e) => {
        if (dotNetHelper) {
            dotNetHelper.invokeMethodAsync('OnKeyDown', e.code);
            // Prevent default for game keys
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

function initializeShaders() {
    // Compile vertex shader
    const vertexShader = gl.createShader(gl.VERTEX_SHADER);
    gl.shaderSource(vertexShader, vertexShaderSource);
    gl.compileShader(vertexShader);
    
    if (!gl.getShaderParameter(vertexShader, gl.COMPILE_STATUS)) {
        console.error('Vertex shader compilation error:', gl.getShaderInfoLog(vertexShader));
        return;
    }
    
    // Compile fragment shader
    const fragmentShader = gl.createShader(gl.FRAGMENT_SHADER);
    gl.shaderSource(fragmentShader, fragmentShaderSource);
    gl.compileShader(fragmentShader);
    
    if (!gl.getShaderParameter(fragmentShader, gl.COMPILE_STATUS)) {
        console.error('Fragment shader compilation error:', gl.getShaderInfoLog(fragmentShader));
        return;
    }
    
    // Link program
    shaderProgram = gl.createProgram();
    gl.attachShader(shaderProgram, vertexShader);
    gl.attachShader(shaderProgram, fragmentShader);
    gl.linkProgram(shaderProgram);
    
    if (!gl.getProgramParameter(shaderProgram, gl.LINK_STATUS)) {
        console.error('Shader program linking error:', gl.getProgramInfoLog(shaderProgram));
        return;
    }
    
    gl.useProgram(shaderProgram);
    
    // Get attribute and uniform locations
    shaderProgram.aPosition = gl.getAttribLocation(shaderProgram, 'aPosition');
    shaderProgram.aTexCoord = gl.getAttribLocation(shaderProgram, 'aTexCoord');
    shaderProgram.uResolution = gl.getUniformLocation(shaderProgram, 'uResolution');
    shaderProgram.uTranslation = gl.getUniformLocation(shaderProgram, 'uTranslation');
    shaderProgram.uColor = gl.getUniformLocation(shaderProgram, 'uColor');
    
    // Enable vertex attributes
    gl.enableVertexAttribArray(shaderProgram.aPosition);
    gl.enableVertexAttribArray(shaderProgram.aTexCoord);
}

function initializeBuffers() {
    // Create vertex buffer for rectangle positions
    vertexBuffer = gl.createBuffer();
    
    // Create texture coordinate buffer
    texCoordBuffer = gl.createBuffer();
    const texCoords = new Float32Array([
        0.0, 0.0,
        1.0, 0.0,
        1.0, 1.0,
        0.0, 1.0
    ]);
    gl.bindBuffer(gl.ARRAY_BUFFER, texCoordBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, texCoords, gl.STATIC_DRAW);
    
    // Create index buffer for rectangles (2 triangles)
    indexBuffer = gl.createBuffer();
    const indices = new Uint16Array([0, 1, 2, 0, 2, 3]);
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indexBuffer);
    gl.bufferData(gl.ELEMENT_ARRAY_BUFFER, indices, gl.STATIC_DRAW);
}

export function render(renderData) {
    if (!gl || !shaderProgram) {
        console.error('WebGL not initialized');
        return;
    }
    
    if (!renderData) {
        console.error('No render data provided');
        return;
    }
    
    const { player, entities, cameraX, level } = renderData;
    
    // Clear the canvas
    gl.clear(gl.COLOR_BUFFER_BIT);
    
    // Set resolution uniform
    gl.uniform2f(shaderProgram.uResolution, canvas.width, canvas.height);
    
    // Apply camera translation
    const cameraOffsetX = -(cameraX || 0) * SCALE;
    
    // Render level
    if (level) {
        renderLevel(level, cameraOffsetX);
    }
    
    // Render entities
    if (entities && entities.length > 0) {
        entities.forEach(entity => renderEntity(entity, cameraOffsetX));
    }
    
    // Render player
    if (player) {
        renderPlayer(player, cameraOffsetX);
    }
}

function renderLevel(map, cameraX) {
    if (!map) return;
    
    const tileColors = {
        1: [0.78, 0.30, 0.05, 1.0], // Ground #C84C0C
        2: [0.72, 0.20, 0.06, 1.0], // Brick #B83410
        3: [0.97, 0.85, 0.13, 1.0], // Question #F8D820
        4: [0.63, 0.63, 0.63, 1.0], // Hit question block (gray)
        5: [0.50, 0.50, 0.50, 1.0], // Hard block (gray)
        6: [0.00, 0.66, 0.00, 1.0], // Pipe left #00A800
        7: [0.00, 0.66, 0.00, 1.0], // Pipe right #00A800
        8: [0.00, 0.66, 0.00, 1.0], // Pipe top left #00A800
        9: [0.00, 0.66, 0.00, 1.0], // Pipe top right #00A800
        12: [0.97, 0.85, 0.13, 1.0] // Coin (gold)
    };
    
    for (let y = 0; y < map.length; y++) {
        if (!map[y]) continue;
        
        for (let x = 0; x < map[y].length; x++) {
            const tile = map[y][x];
            if (tile === 0) continue;
            
            const color = tileColors[tile] || [1.0, 1.0, 1.0, 1.0];
            const tileX = x * TILE_SIZE * SCALE;
            const tileY = y * TILE_SIZE * SCALE;
            
            drawRectangle(tileX, tileY, TILE_SIZE * SCALE, TILE_SIZE * SCALE, color, cameraX);
            
            // Add question mark for question blocks
            if (tile === 3) {
                const qColor = [0.0, 0.0, 0.0, 1.0]; // Black
                const centerX = tileX + (TILE_SIZE * SCALE) / 2;
                const centerY = tileY + (TILE_SIZE * SCALE) / 2;
                // Simple question mark representation (small rectangle)
                drawRectangle(centerX - 4, centerY - 8, 8, 16, qColor, cameraX);
            }
            
            // Render coins as circles (well, small squares with shine)
            if (tile === 12) {
                const coinSize = TILE_SIZE * SCALE * 0.5;
                const coinX = tileX + (TILE_SIZE * SCALE - coinSize) / 2;
                const coinY = tileY + (TILE_SIZE * SCALE - coinSize) / 2;
                // Gold coin
                drawRectangle(coinX, coinY, coinSize, coinSize, color, cameraX);
                // White shine
                const shineSize = coinSize * 0.3;
                drawRectangle(coinX + shineSize * 0.5, coinY + shineSize * 0.5, shineSize, shineSize, [1.0, 1.0, 1.0, 1.0], cameraX);
            }
        }
    }
}

function renderPlayer(player, cameraX) {
    const x = Math.floor(player.pos.x * SCALE);
    const y = Math.floor(player.pos.y * SCALE);
    const w = player.width * SCALE;
    const h = player.height * SCALE;
    
    // Mario colors
    const red = [0.97, 0.22, 0.00, 1.0];    // #F83800
    const skin = [1.00, 0.63, 0.27, 1.0];   // #FFA044
    const brown = [0.53, 0.44, 0.00, 1.0];  // #887000
    
    // Hat
    drawRectangle(x + 2, y, w - 4, 4, red, cameraX);
    
    // Face
    drawRectangle(x + 2, y + 4, w - 4, 6, skin, cameraX);
    
    // Body
    drawRectangle(x + 2, y + 10, w - 4, 4, red, cameraX);
    
    // Legs
    drawRectangle(x + 2, y + 14, 4, 2, brown, cameraX);
    drawRectangle(x + w - 6, y + 14, 4, 2, brown, cameraX);
}

function renderEntity(entity, cameraX) {
    const x = Math.floor(entity.pos.x * SCALE);
    const y = Math.floor(entity.pos.y * SCALE);
    const w = (entity.width || 16) * SCALE;
    const h = (entity.height || 16) * SCALE;
    
    const white = [1.00, 1.00, 1.00, 1.0];
    const black = [0.00, 0.00, 0.00, 1.0];
    
    // EntityType.MUSHROOM = 3
    if (entity.type === 3 || entity.state === 'mushroom') {
        const red = [0.97, 0.06, 0.09, 1.0];    // Red cap
        const cream = [1.00, 0.97, 0.86, 1.0];  // Cream stem
        
        // Mushroom cap (red with white spots)
        drawRectangle(x, y, w, h * 0.6, red, cameraX);
        
        // White spots on cap
        const spotSize = w * 0.25;
        drawRectangle(x + w * 0.2, y + h * 0.15, spotSize, spotSize * 0.8, white, cameraX);
        drawRectangle(x + w * 0.55, y + h * 0.15, spotSize, spotSize * 0.8, white, cameraX);
        
        // Mushroom stem (cream/white)
        drawRectangle(x + w * 0.3, y + h * 0.6, w * 0.4, h * 0.4, cream, cameraX);
        
        // Eyes
        drawRectangle(x + w * 0.35, y + h * 0.35, w * 0.1, h * 0.1, black, cameraX);
        drawRectangle(x + w * 0.55, y + h * 0.35, w * 0.1, h * 0.1, black, cameraX);
    }
    // EntityType.KOOPA = 2 or rex/koopa states
    else if (entity.type === 2 || entity.state === 'koopa' || entity.state === 'rex') {
        const purple = [0.38, 0.47, 0.85, 1.0];  // Rex color
        const green = [0.35, 0.78, 0.44, 1.0];   // Koopa color
        const yellow = [0.94, 0.82, 0.35, 1.0];  // Belly color
        
        const bodyColor = entity.state === 'rex' ? purple : green;
        
        // Body
        drawRectangle(x, y + h * 0.3, w, h * 0.7, bodyColor, cameraX);
        
        // Belly
        drawRectangle(x + w * 0.2, y + h * 0.4, w * 0.6, h * 0.5, yellow, cameraX);
        
        // Head
        drawRectangle(x + w * 0.2, y, w * 0.6, h * 0.4, bodyColor, cameraX);
        
        // Eyes
        drawRectangle(x + w * 0.3, y + h * 0.15, w * 0.15, h * 0.15, white, cameraX);
        drawRectangle(x + w * 0.55, y + h * 0.15, w * 0.15, h * 0.15, white, cameraX);
        
        // Pupils
        drawRectangle(x + w * 0.35, y + h * 0.18, w * 0.08, h * 0.1, black, cameraX);
        drawRectangle(x + w * 0.6, y + h * 0.18, w * 0.08, h * 0.1, black, cameraX);
    }
    // EntityType.GOOMBA = 1 (fallback)
    else {
        const brown = [0.53, 0.44, 0.00, 1.0];
        
        // Body
        drawRectangle(x, y, w, h, brown, cameraX);
        
        // Eyes
        drawRectangle(x + 3, y + 4, 4, 4, white, cameraX);
        drawRectangle(x + w - 7, y + 4, 4, 4, white, cameraX);
        
        // Pupils
        drawRectangle(x + 4, y + 5, 2, 2, black, cameraX);
        drawRectangle(x + w - 6, y + 5, 2, 2, black, cameraX);
    }
}

function drawRectangle(x, y, width, height, color, cameraX) {
    // Create vertices for rectangle
    const x1 = x;
    const y1 = y;
    const x2 = x + width;
    const y2 = y + height;
    
    const vertices = new Float32Array([
        x1, y1,
        x2, y1,
        x2, y2,
        x1, y2
    ]);
    
    // Bind and update vertex buffer
    gl.bindBuffer(gl.ARRAY_BUFFER, vertexBuffer);
    gl.bufferData(gl.ARRAY_BUFFER, vertices, gl.DYNAMIC_DRAW);
    gl.vertexAttribPointer(shaderProgram.aPosition, 2, gl.FLOAT, false, 0, 0);
    
    // Bind texture coordinates
    gl.bindBuffer(gl.ARRAY_BUFFER, texCoordBuffer);
    gl.vertexAttribPointer(shaderProgram.aTexCoord, 2, gl.FLOAT, false, 0, 0);
    
    // Set uniforms
    gl.uniform2f(shaderProgram.uTranslation, cameraX, 0);
    gl.uniform4fv(shaderProgram.uColor, color);
    
    // Bind index buffer and draw
    gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, indexBuffer);
    gl.drawElements(gl.TRIANGLES, 6, gl.UNSIGNED_SHORT, 0);
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

// Sound support - simple Web Audio API implementation
const audioContext = new (window.AudioContext || window.webkitAudioContext)();
const sounds = {};

// Simple sound generator using Web Audio API
function createSound(type, frequency, duration) {
    const oscillator = audioContext.createOscillator();
    const gainNode = audioContext.createGain();
    
    oscillator.connect(gainNode);
    gainNode.connect(audioContext.destination);
    
    oscillator.type = type;
    oscillator.frequency.value = frequency;
    
    gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
    gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + duration);
    
    oscillator.start(audioContext.currentTime);
    oscillator.stop(audioContext.currentTime + duration);
}

// Play sound effect
export function playSound(soundName) {
    try {
        switch(soundName) {
            case 'jump':
                // Jump sound - rising pitch
                createSound('square', 300, 0.1);
                break;
            case 'coin':
                // Coin sound - two-tone high pitch
                createSound('square', 988, 0.05);
                setTimeout(() => createSound('square', 1319, 0.1), 50);
                break;
            case 'stomp':
                // Enemy stomp - thump sound
                createSound('square', 150, 0.1);
                break;
            case 'powerup':
                // Power-up collect - ascending tones
                createSound('sine', 523, 0.1);
                setTimeout(() => createSound('sine', 659, 0.1), 100);
                setTimeout(() => createSound('sine', 784, 0.1), 200);
                setTimeout(() => createSound('sine', 1047, 0.2), 300);
                break;
            case 'powerup-appears':
                // Power-up appears - quick ascending scale
                createSound('sine', 523, 0.08);
                setTimeout(() => createSound('sine', 659, 0.08), 80);
                setTimeout(() => createSound('sine', 784, 0.08), 160);
                break;
            default:
                console.log('Unknown sound:', soundName);
        }
    } catch (error) {
        console.log('Sound error:', error);
    }
}
