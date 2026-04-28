// ═══════════════════════════════════════════════════════════════════════
//  Form1_Audio.cs  — partial class Form1
//
//  Фоновая музыка через WinAPI mciSendString (winmm.dll).
//  Никаких внешних зависимостей, никакого COM.
//  InitMusic() безопасно вызывать несколько раз — повторный вызов игнорируется.
// ═══════════════════════════════════════════════════════════════════════

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace TheGatekeeper
{
    public partial class Form1
    {
        // ─── WinAPI ──────────────────────────────────────────────────────────
        [DllImport("winmm.dll", CharSet = CharSet.Auto)]
        private static extern int mciSendString(
            string command, StringBuilder returnString,
            int returnLength, IntPtr hwndCallback);

        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        // ─── Состояние ───────────────────────────────────────────────────────
        private static int _musicVolume = 60;
        private static bool _musicEnabled = true;
        private static bool _musicLoaded = false;
        private const string _musicAlias = "bgmusic";
        private static Timer _loopTimer;

        // ════════════════════════════════════════════════════════════════════
        //  ИНИЦИАЛИЗАЦИЯ — безопасна при повторном вызове
        // ════════════════════════════════════════════════════════════════════
        internal static void InitMusic()
        {
            if (_musicLoaded) return;   // уже запущена — не перезапускаем
            try
            {
                string mp3 = Path.Combine(Application.StartupPath, "Music", "Tin_Choir.mp3");
                if (!File.Exists(mp3)) return;

                mciSendString("open \"" + mp3 + "\" type mpegvideo alias " + _musicAlias, null, 0, IntPtr.Zero);
                _musicLoaded = true;

                ApplyVolume();
                mciSendString("play " + _musicAlias + " from 0", null, 0, IntPtr.Zero);

                _loopTimer = new Timer { Interval = 500 };
                _loopTimer.Tick += LoopCheck;
                _loopTimer.Start();
            }
            catch
            {
                _musicLoaded = false;
            }
        }

        // ════════════════════════════════════════════════════════════════════
        //  LOOP — перезапуск когда трек закончился
        // ════════════════════════════════════════════════════════════════════
        private static void LoopCheck(object sender, EventArgs e)
        {
            if (!_musicLoaded || !_musicEnabled) return;
            try
            {
                var buf = new StringBuilder(128);
                mciSendString("status " + _musicAlias + " mode", buf, buf.Capacity, IntPtr.Zero);
                if (buf.ToString().Trim() == "stopped")
                    mciSendString("play " + _musicAlias + " from 0", null, 0, IntPtr.Zero);
            }
            catch { }
        }

        // ════════════════════════════════════════════════════════════════════
        //  ПУБЛИЧНЫЙ API
        // ════════════════════════════════════════════════════════════════════
        internal static void SetMusicVolume(int volume)
        {
            _musicVolume = Math.Max(0, Math.Min(100, volume));
            if (_musicEnabled) ApplyVolume();
        }

        internal static int GetMusicVolume() => _musicVolume;
        internal static bool GetMusicEnabled() => _musicEnabled;

        internal static void SetMusicEnabled(bool enabled)
        {
            _musicEnabled = enabled;
            if (!_musicLoaded) return;

            if (enabled)
            {
                ApplyVolume();
                mciSendString("play " + _musicAlias + " from 0", null, 0, IntPtr.Zero);
                _loopTimer?.Start();
            }
            else
            {
                mciSendString("pause " + _musicAlias, null, 0, IntPtr.Zero);
                waveOutSetVolume(IntPtr.Zero, 0);
                _loopTimer?.Stop();
            }
        }

        // ─── Громкость 0-100 → 0x0000-0xFFFF стерео ─────────────────────────
        private static void ApplyVolume()
        {
            try
            {
                uint vol = (uint)(_musicVolume * 0xFFFF / 100);
                uint stereo = (vol & 0xFFFF) | ((vol & 0xFFFF) << 16);
                waveOutSetVolume(IntPtr.Zero, stereo);
            }
            catch { }
        }

        // ════════════════════════════════════════════════════════════════════
        //  ОСВОБОЖДЕНИЕ — только при полном выходе из приложения
        // ════════════════════════════════════════════════════════════════════
        internal static void DisposeMusic()
        {
            try
            {
                _loopTimer?.Stop();
                _loopTimer?.Dispose();
                _loopTimer = null;

                if (_musicLoaded)
                {
                    mciSendString("stop " + _musicAlias, null, 0, IntPtr.Zero);
                    mciSendString("close " + _musicAlias, null, 0, IntPtr.Zero);
                    _musicLoaded = false;
                }

                waveOutSetVolume(IntPtr.Zero, 0xFFFFFFFF);
            }
            catch { }
        }
    }
}