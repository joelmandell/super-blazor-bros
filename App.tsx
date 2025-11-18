
import React, { useState, useEffect } from 'react';
import GameCanvas from './components/GameCanvas';
import { GameStatus, LevelData, GameStats, EntityType } from './types';
import { DEFAULT_LEVEL_MAP, SCREEN_WIDTH, SCALE, COLORS } from './constants';
import { generateLevel } from './services/geminiService';
import { audioService } from './services/audioService';

export default function App() {
  const [status, setStatus] = useState<GameStatus>(GameStatus.MENU);
  const [stats, setStats] = useState<GameStats>({
    score: 0,
    coins: 0,
    world: '1-1',
    time: 400,
    lives: 3
  });
  const [levelData, setLevelData] = useState<LevelData>({
    map: DEFAULT_LEVEL_MAP,
    entities: [],
    backgroundColor: COLORS.SKY
  });
  const [loadingText, setLoadingText] = useState('');
  
  // API Key State
  const [apiKey, setApiKey] = useState('');
  const [showKeyInput, setShowKeyInput] = useState(false);

  useEffect(() => {
    const storedKey = localStorage.getItem('gemini_api_key');
    if (storedKey) {
      setApiKey(storedKey);
    }
  }, []);

  const saveApiKey = (key: string) => {
    setApiKey(key);
    localStorage.setItem('gemini_api_key', key);
    setShowKeyInput(false);
  };

  // Timer
  useEffect(() => {
    let interval: any;
    if (status === GameStatus.PLAYING) {
      interval = setInterval(() => {
        setStats(prev => {
          if (prev.time <= 0) {
             return prev; // Logic handled in game loop eventually
          }
          return { ...prev, time: prev.time - 1 };
        });
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [status]);

  const startGame = () => {
    setStats(s => ({ ...s, score: 0, coins: 0, time: 400, lives: 3, world: '1-1' }));
    resetLevel(DEFAULT_LEVEL_MAP);
    setStatus(GameStatus.PLAYING);
  };

  const resetLevel = (map: number[][]) => {
    let entities = [];
    
    // Basic Goomba placement for default map
    if (map === DEFAULT_LEVEL_MAP) {
      // 1-1 Classic positions (approx)
      const goombaX = [22, 40, 51, 52.5, 80, 82, 97, 98.5, 114, 115.5, 124, 125.5, 174, 175.5];
      goombaX.forEach((x, i) => {
        entities.push({
          id: i + 100,
          type: EntityType.GOOMBA,
          pos: { x: x * 16, y: 10 * 16 }, // slightly above ground to fall in
          vel: { x: -0.5, y: 0 },
          width: 16,
          height: 16,
          dead: false,
          grounded: false,
          direction: -1
        });
      });
    }

    setLevelData({
      map,
      entities: entities as any,
      backgroundColor: COLORS.SKY
    });
  };

  const handleAiGenerate = async () => {
    console.log('[AI Generate] Funktion startad');
    
    if (!apiKey) {
      console.warn('[AI Generate] Ingen API-nyckel hittad, visar input-dialog');
      setShowKeyInput(true);
      setStatus(GameStatus.MENU); // Return to menu to show input
      return;
    }

    console.log('[AI Generate] API-nyckel finns:', apiKey ? `${apiKey.substring(0, 10)}...` : 'SAKNAS');
    console.log('[AI Generate] Startar generering av AI-bana...');
    
    setLoadingText("Gemini bygger en bana...");
    setStatus(GameStatus.LOADING);
    
    try {
      const startTime = Date.now();
      const data = await generateLevel(apiKey);
      const duration = Date.now() - startTime;
      
      console.log(`[AI Generate] Generering klar efter ${duration}ms`);
      
      if (data) {
        console.log('[AI Generate] Bana genererad framgångsrikt!');
        console.log('[AI Generate] Bana-data:', {
          mapRows: data.map.length,
          mapCols: data.map[0]?.length || 0,
          entitiesCount: data.entities?.length || 0,
          backgroundColor: data.backgroundColor
        });
        setLevelData(data);
        setStats(s => ({ ...s, score: 0, coins: 0, time: 400, world: '1-AI', lives: 3 }));
        setStatus(GameStatus.PLAYING);
        console.log('[AI Generate] Spelstatus ändrad till PLAYING');
      } else {
        console.error('[AI Generate] Generering misslyckades - ingen data returnerad');
        alert("Kunde inte generera bana. Kontrollera API-nyckel och försök igen.");
        setStatus(GameStatus.MENU);
      }
    } catch (error) {
      console.error('[AI Generate] Fel vid generering:', error);
      alert("Ett fel uppstod vid generering av bana. Försök igen.");
      setStatus(GameStatus.MENU);
    }
  };

  const handleDie = () => {
    setTimeout(() => {
        setStats(prev => {
            const newLives = prev.lives - 1;
            if (newLives < 0) {
                setStatus(GameStatus.GAME_OVER);
                return prev;
            } else {
                setStatus(GameStatus.LOADING);
                setTimeout(() => setStatus(GameStatus.PLAYING), 1000);
                return { ...prev, lives: newLives, time: 400 };
            }
        });
    }, 2500); // Wait for death anim
  };

  const handleWin = () => {
      // Audio handled in Canvas now for timing
      setStatus(GameStatus.VICTORY);
  };

  // Touch/Click Control Helpers
  const sendKey = (code: string, type: 'keydown' | 'keyup') => {
    // Must set bubbles: true for React or global listeners to sometimes catch it properly
    window.dispatchEvent(new KeyboardEvent(type, { code, bubbles: true, cancelable: true }));
  };

  const ControlButton = ({ code, label, color = "bg-gray-700", subLabel }: any) => (
    <div className="flex flex-col items-center">
      <button
        className={`w-14 h-14 md:w-16 md:h-16 rounded-full ${color} border-b-4 border-black/30 active:border-b-0 active:translate-y-1 text-white font-bold shadow-lg active:shadow-none transition-all select-none touch-none flex items-center justify-center text-xl`}
        style={{ fontFamily: '"Press Start 2P"' }}
        onMouseDown={() => sendKey(code, 'keydown')}
        onMouseUp={() => sendKey(code, 'keyup')}
        onMouseLeave={() => sendKey(code, 'keyup')}
        onTouchStart={(e) => { e.preventDefault(); sendKey(code, 'keydown'); }}
        onTouchEnd={(e) => { e.preventDefault(); sendKey(code, 'keyup'); }}
      >
        {label}
      </button>
      {subLabel && <span className="text-[10px] text-gray-400 mt-2 font-bold tracking-widest">{subLabel}</span>}
    </div>
  );

  return (
    <div className="min-h-screen bg-[#202020] flex flex-col items-center justify-center p-2 font-sans select-none overflow-hidden">
      
      {/* Header / HUD */}
      <div className="flex justify-between w-full max-w-[768px] mb-2 text-white uppercase text-lg md:text-xl tracking-widest px-4" style={{fontFamily: '"Press Start 2P"'}}>
        <div className="flex flex-col">
          <span className="text-xs md:text-sm opacity-80">MARIO</span>
          <span>{stats.score.toString().padStart(6, '0')}</span>
        </div>
        <div className="flex flex-col items-center">
          <span className="text-xs md:text-sm opacity-80">MYNT</span>
          <span>x{stats.coins.toString().padStart(2, '0')}</span>
        </div>
        <div className="flex flex-col items-center">
           <span className="text-xs md:text-sm opacity-80">VÄRLD</span>
           <span>{stats.world}</span>
        </div>
        <div className="flex flex-col items-end">
           <span className="text-xs md:text-sm opacity-80">TID</span>
           <span>{stats.time.toString().padStart(3, '0')}</span>
        </div>
      </div>

      {/* Game Container */}
      <div className="relative w-full max-w-[768px] shadow-2xl border-4 border-gray-800 rounded-lg overflow-hidden bg-black">
        <GameCanvas 
          status={status} 
          levelData={levelData} 
          onScore={(p) => setStats(s => ({...s, score: s.score + p}))}
          onCoin={() => setStats(s => ({...s, coins: s.coins + 1}))}
          onDie={handleDie}
          onWin={handleWin}
        />

        {/* Menu Overlay */}
        {status === GameStatus.MENU && (
          <div className="absolute inset-0 bg-black/90 flex flex-col items-center justify-center text-white z-10 p-4">
            <h1 className="text-2xl md:text-5xl mb-8 text-[#F83800] drop-shadow-[4px_4px_0_rgba(255,255,255,0.2)] text-center" style={{fontFamily: '"Press Start 2P"'}}>SUPER REACT BROS</h1>
            
            {!showKeyInput ? (
              <>
                <button 
                  onClick={startGame}
                  className="px-6 py-4 bg-white text-black hover:bg-[#F8D820] mb-4 text-sm md:text-lg transition-colors border-4 border-transparent hover:border-white w-full max-w-sm uppercase"
                  style={{fontFamily: '"Press Start 2P"'}}
                >
                  STARTA SPEL (1-1)
                </button>
                <button 
                  onClick={handleAiGenerate}
                  className="px-6 py-3 bg-indigo-600 text-white hover:bg-indigo-500 text-xs md:text-sm transition-colors flex items-center justify-center gap-2 w-full max-w-sm uppercase border-4 border-transparent hover:border-white"
                  style={{fontFamily: '"Press Start 2P"'}}
                >
                  <span>✨</span> SKAPA AI-BANA
                </button>
                <button 
                  onClick={() => setShowKeyInput(true)}
                  className="mt-4 text-gray-500 hover:text-white text-[10px] uppercase underline"
                  style={{fontFamily: '"Press Start 2P"'}}
                >
                  {apiKey ? 'Ändra API-nyckel' : 'Ange API-nyckel'}
                </button>

                <div className="mt-8 text-center max-w-md text-xs md:text-sm leading-6 text-gray-400">
                  <p className="text-yellow-400 mb-2">KONTROLLER:</p>
                  <p>PILLAR / D-PAD för att gå</p>
                  <p><span className="text-white">SPACE / A</span> för att HOPPA</p>
                  <p><span className="text-white">SHIFT / B</span> för att SPRINGA/SKJUTA</p>
                </div>
              </>
            ) : (
              <div className="w-full max-w-sm bg-gray-900 p-6 border-4 border-gray-700">
                <h2 className="text-yellow-400 mb-4 text-center text-sm" style={{fontFamily: '"Press Start 2P"'}}>Gemini API Nyckel</h2>
                <p className="text-[10px] text-gray-400 mb-4 leading-4">
                  För att använda AI-funktionerna behöver du en gratis API-nyckel från Google AI Studio. Nyckeln sparas i din webbläsare.
                </p>
                <input 
                  type="password"
                  className="w-full p-3 bg-black border-2 border-gray-500 text-white mb-4 font-mono text-xs focus:border-yellow-400 outline-none"
                  placeholder="Klistra in nyckel här..."
                  autoFocus
                  value={apiKey}
                  onChange={(e) => setApiKey(e.target.value)}
                />
                <div className="flex gap-2">
                  <button 
                    onClick={() => saveApiKey(apiKey)}
                    className="flex-1 bg-green-600 text-white py-3 text-xs uppercase border-2 border-transparent hover:border-white"
                    style={{fontFamily: '"Press Start 2P"'}}
                  >
                    Spara
                  </button>
                  <button 
                    onClick={() => setShowKeyInput(false)}
                    className="flex-1 bg-red-600 text-white py-3 text-xs uppercase border-2 border-transparent hover:border-white"
                    style={{fontFamily: '"Press Start 2P"'}}
                  >
                    Avbryt
                  </button>
                </div>
                <a href="https://aistudio.google.com/app/apikey" target="_blank" rel="noreferrer" className="block mt-4 text-center text-[10px] text-blue-400 hover:text-blue-300 underline">
                  Hämta nyckel här
                </a>
              </div>
            )}
          </div>
        )}

        {/* Loading Overlay */}
        {status === GameStatus.LOADING && (
          <div className="absolute inset-0 bg-black flex flex-col items-center justify-center text-white z-10">
            <p className="text-xl md:text-2xl animate-pulse uppercase" style={{fontFamily: '"Press Start 2P"'}}>
              {loadingText || "x  " + stats.lives}
            </p>
          </div>
        )}

        {/* Game Over Overlay */}
        {status === GameStatus.GAME_OVER && (
          <div className="absolute inset-0 bg-black flex flex-col items-center justify-center text-white z-10">
            <p className="text-4xl text-red-500 mb-8" style={{fontFamily: '"Press Start 2P"'}}>GAME OVER</p>
            <button onClick={() => setStatus(GameStatus.MENU)} className="text-white hover:text-yellow-400 text-xl" style={{fontFamily: '"Press Start 2P"'}}>MENY</button>
          </div>
        )}

        {/* Victory Overlay */}
        {status === GameStatus.VICTORY && (
          <div className="absolute inset-0 bg-blue-500 flex flex-col items-center justify-center text-white z-10">
            <p className="text-3xl text-yellow-300 mb-8 text-center leading-tight drop-shadow-md" style={{fontFamily: '"Press Start 2P"'}}>BANA KLARAD!</p>
            <p className="mb-8 text-xl" style={{fontFamily: '"Press Start 2P"'}}>POÄNG: {stats.score}</p>
            <div className="flex flex-col gap-4 w-full max-w-xs px-4">
              <button onClick={startGame} className="bg-white text-black border-4 border-black px-4 py-3 hover:bg-gray-200 transition uppercase font-bold" style={{fontFamily: '"Press Start 2P"'}}>SPELA IGEN</button>
              <button onClick={handleAiGenerate} className="bg-yellow-400 text-black border-4 border-black px-4 py-3 hover:bg-yellow-300 transition uppercase font-bold" style={{fontFamily: '"Press Start 2P"'}}>NÄSTA BANA (AI)</button>
            </div>
          </div>
        )}
      </div>

      {/* On-Screen Controls */}
      <div className="mt-6 w-full max-w-[768px] flex justify-between items-center px-4 md:px-12 pb-8 gap-8 select-none">
         {/* D-PAD */}
         <div className="bg-gray-800 p-3 rounded-full shadow-inner border border-gray-700">
           <div className="flex gap-2">
              <ControlButton code="ArrowLeft" label="←" />
              <ControlButton code="ArrowRight" label="→" />
           </div>
         </div>

         {/* Brand */}
         <div className="hidden md:block text-gray-600 font-bold italic text-2xl opacity-20 tracking-widest">NINTENDO</div>

         {/* ACTIONS */}
         <div className="bg-gray-800 p-3 rounded-full shadow-inner border border-gray-700 px-6">
           <div className="flex gap-6 md:gap-8 translate-y-2">
              <ControlButton code="ShiftLeft" label="B" color="bg-[#cc0000]" subLabel="SPRING" />
              <div className="-translate-y-4">
                <ControlButton code="Space" label="A" color="bg-[#cc0000]" subLabel="HOPPA" />
              </div>
           </div>
         </div>
      </div>
    </div>
  );
}
