// Sprite Loader - Handles loading and caching sprite images

// Cache for loaded images
const imageCache = new Map();

/**
 * Check if an image exists and can be loaded
 * @param {string} url - The URL of the image to check
 * @returns {Promise<boolean>} True if image exists and loads successfully
 */
window.checkImageExists = function(url) {
    return new Promise((resolve) => {
        const img = new Image();
        img.onload = () => resolve(true);
        img.onerror = () => resolve(false);
        img.src = url;
    });
};

/**
 * Preload an image and cache it
 * @param {string} name - The name/key for the sprite
 * @param {string} url - The URL of the image
 * @returns {Promise<boolean>} True if loaded successfully
 */
window.preloadSprite = function(name, url) {
    return new Promise((resolve) => {
        if (imageCache.has(name)) {
            resolve(true);
            return;
        }

        const img = new Image();
        img.onload = () => {
            imageCache.set(name, img);
            console.log(`Sprite loaded: ${name}`);
            resolve(true);
        };
        img.onerror = () => {
            console.warn(`Failed to load sprite: ${name} from ${url}`);
            resolve(false);
        };
        img.src = url;
    });
};

/**
 * Preload multiple sprites
 * @param {Object} sprites - Object mapping sprite names to URLs
 * @returns {Promise<Object>} Object mapping sprite names to load success status
 */
window.preloadSprites = async function(sprites) {
    const results = {};
    const promises = [];

    for (const [name, url] of Object.entries(sprites)) {
        promises.push(
            window.preloadSprite(name, url).then(success => {
                results[name] = success;
            })
        );
    }

    await Promise.all(promises);
    return results;
};

/**
 * Get a cached sprite image
 * @param {string} name - The name of the sprite
 * @returns {HTMLImageElement|null} The cached image or null if not found
 */
window.getSprite = function(name) {
    return imageCache.get(name) || null;
};

/**
 * Check if a sprite is loaded in cache
 * @param {string} name - The name of the sprite
 * @returns {boolean} True if sprite is in cache
 */
window.isSpriteLoaded = function(name) {
    return imageCache.has(name);
};

/**
 * Clear the sprite cache
 */
window.clearSpriteCache = function() {
    imageCache.clear();
    console.log('Sprite cache cleared');
};

/**
 * Get sprite cache statistics
 * @returns {Object} Cache statistics
 */
window.getSpriteStats = function() {
    return {
        count: imageCache.size,
        sprites: Array.from(imageCache.keys())
    };
};

export { 
    checkImageExists, 
    preloadSprite, 
    preloadSprites, 
    getSprite, 
    isSpriteLoaded, 
    clearSpriteCache,
    getSpriteStats 
};
