/* ── SoundSphere Global In-Browser Audio Player ── */

const SoundPlayer = {
  audio: null,
  isPlaying: false,
  currentTrack: null,
  playlist: [],
  currentIndex: -1,

  init() {
    this.audio = document.getElementById("global-audio-element");
    if (!this.audio) return;

    this.bindEvents();
    this.restoreState();
  },

  bindEvents() {
    const playBtn = document.getElementById("player-play-btn");
    const prevBtn = document.getElementById("player-prev-btn");
    const nextBtn = document.getElementById("player-next-btn");
    const progressSlider = document.getElementById("player-progress-slider");
    const volumeSlider = document.getElementById("player-volume-slider");
    const muteBtn = document.getElementById("player-mute-btn");

    if (playBtn) {
      playBtn.addEventListener("click", () => this.togglePlay());
    }

    if (prevBtn) {
      prevBtn.addEventListener("click", () => this.prev());
    }

    if (nextBtn) {
      nextBtn.addEventListener("click", () => this.next());
    }

    if (progressSlider) {
      progressSlider.addEventListener("input", (e) => {
        if (!this.audio || !this.audio.duration) return;
        const seekTime = (e.target.value / 100) * this.audio.duration;
        this.audio.currentTime = seekTime;
      });
    }

    if (volumeSlider) {
      volumeSlider.addEventListener("input", (e) => {
        if (!this.audio) return;
        this.audio.volume = e.target.value / 100;
        localStorage.setItem("soundsphere_volume", this.audio.volume);
      });
    }

    if (muteBtn) {
      muteBtn.addEventListener("click", () => {
        if (!this.audio) return;
        this.audio.muted = !this.audio.muted;
        muteBtn.innerHTML = this.audio.muted ? "🔇" : "🔊";
      });
    }

    this.audio.addEventListener("timeupdate", () => this.onTimeUpdate());
    this.audio.addEventListener("ended", () => this.onTrackEnded());
    this.audio.addEventListener("play", () => this.updatePlayIcon(true));
    this.audio.addEventListener("pause", () => this.updatePlayIcon(false));

    // Global keyboard shortcut: Space = Play/Pause when not focused on an input
    document.addEventListener("keydown", (e) => {
      if (e.code === "Space" && e.target.tagName !== "INPUT" && e.target.tagName !== "TEXTAREA") {
        e.preventDefault();
        this.togglePlay();
      }
    });
  },

  playTrack(track) {
    if (!this.audio) return;

    this.currentTrack = track;
    this.audio.src = `/Media/StreamAudio/${track.id}`;
    this.audio.play().catch(err => {
      console.warn("Autoplay prevented or stream error:", err);
    });

    this.updateUI(track);
    this.saveState(track);
  },

  togglePlay() {
    if (!this.audio || !this.audio.src) return;
    if (this.audio.paused) {
      this.audio.play();
    } else {
      this.audio.pause();
    }
  },

  prev() {
    if (this.playlist.length > 0 && this.currentIndex > 0) {
      this.currentIndex--;
      this.playTrack(this.playlist[this.currentIndex]);
    }
  },

  next() {
    if (this.playlist.length > 0 && this.currentIndex < this.playlist.length - 1) {
      this.currentIndex++;
      this.playTrack(this.playlist[this.currentIndex]);
    }
  },

  onTimeUpdate() {
    const curTimeEl = document.getElementById("player-current-time");
    const durTimeEl = document.getElementById("player-duration-time");
    const slider = document.getElementById("player-progress-slider");

    if (curTimeEl && this.audio.currentTime) {
      curTimeEl.textContent = this.formatTime(this.audio.currentTime);
    }
    if (durTimeEl && this.audio.duration && !isNaN(this.audio.duration)) {
      durTimeEl.textContent = this.formatTime(this.audio.duration);
    }
    if (slider && this.audio.duration) {
      slider.value = (this.audio.currentTime / this.audio.duration) * 100;
    }
  },

  onTrackEnded() {
    this.next();
  },

  updatePlayIcon(playing) {
    this.isPlaying = playing;
    const playBtn = document.getElementById("player-play-btn");
    if (playBtn) {
      playBtn.innerHTML = playing ? "⏸" : "▶";
    }
  },

  updateUI(track) {
    const titleEl = document.getElementById("player-title");
    const artistEl = document.getElementById("player-artist");
    const thumbEl = document.getElementById("player-thumb");
    const playerBar = document.getElementById("global-player-bar");

    if (titleEl) titleEl.textContent = track.title;
    if (artistEl) artistEl.textContent = track.artist;
    if (thumbEl && track.coverUrl) thumbEl.src = track.coverUrl;
    if (playerBar) playerBar.style.display = "flex";

    // Media Session API for iOS/Android Lock-Screen Player
    if ("mediaSession" in navigator) {
      navigator.mediaSession.metadata = new MediaMetadata({
        title: track.title,
        artist: track.artist || "SoundSphere",
        album: "SoundSphere Streaming",
        artwork: [
          { src: track.coverUrl || "/images/default-cover.jpg", sizes: "96x96", type: "image/jpeg" },
          { src: track.coverUrl || "/images/default-cover.jpg", sizes: "512x512", type: "image/jpeg" }
        ]
      });

      navigator.mediaSession.setActionHandler("play", () => this.togglePlay());
      navigator.mediaSession.setActionHandler("pause", () => this.togglePlay());
      navigator.mediaSession.setActionHandler("previoustrack", () => this.prev());
      navigator.mediaSession.setActionHandler("nexttrack", () => this.next());
    }
  },

  formatTime(seconds) {
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return `${mins}:${secs < 10 ? "0" : ""}${secs}`;
  },

  saveState(track) {
    sessionStorage.setItem("soundsphere_active_track", JSON.stringify(track));
  },

  restoreState() {
    const savedVol = localStorage.getItem("soundsphere_volume");
    if (savedVol && this.audio) {
      this.audio.volume = parseFloat(savedVol);
      const volSlider = document.getElementById("player-volume-slider");
      if (volSlider) volSlider.value = parseFloat(savedVol) * 100;
    }

    const savedTrack = sessionStorage.getItem("soundsphere_active_track");
    if (savedTrack) {
      try {
        const track = JSON.parse(savedTrack);
        this.updateUI(track);
      } catch (e) {}
    }
  }
};

/* ── Toast Notifications ── */
const Toast = {
  icons: { success: "✓", error: "✕", info: "ℹ", warning: "⚠" },

  show(title, message, type = "info", duration = 4000) {
    const container = document.getElementById("toast-container");
    if (!container) return;

    const el = document.createElement("div");
    el.className = `toast-notification toast-${type}`;
    el.innerHTML = `
      <span class="toast-icon">${this.icons[type] || ""}</span>
      <div class="toast-content">
        <div class="toast-title" style="font-weight:700;">${this.escape(title)}</div>
        ${message ? `<div class="toast-message" style="font-size:0.85rem; opacity:0.85;">${this.escape(message)}</div>` : ""}
      </div>
      <button class="toast-dismiss" onclick="this.closest('.toast-notification').remove()" style="background:none;border:none;color:#fff;cursor:pointer;">×</button>
    `;
    container.appendChild(el);

    if (duration > 0) {
      setTimeout(() => el.remove(), duration);
    }
  },

  success(title, msg, dur) { this.show(title, msg, "success", dur); },
  error(title, msg, dur) { this.show(title, msg, "error", dur); },
  info(title, msg, dur) { this.show(title, msg, "info", dur); },
  warning(title, msg, dur) { this.show(title, msg, "warning", dur); },

  escape(str) {
    if (!str) return "";
    const div = document.createElement("div");
    div.appendChild(document.createTextNode(str));
    return div.innerHTML;
  }
};

/* ── Loading Overlay ── */
const Loading = {
  show() {
    const el = document.getElementById("loading-overlay");
    if (el) el.classList.add("active");
  },
  hide() {
    const el = document.getElementById("loading-overlay");
    if (el) el.classList.remove("active");
  }
};

/* ── Mobile Navigation Active State ── */
function highlightActiveMobileNav() {
  const currentPath = window.location.pathname.toLowerCase();
  document.querySelectorAll(".mobile-nav-item").forEach(item => {
    const route = (item.dataset.navRoute || "").toLowerCase();
    if (route === "home" && (currentPath === "/" || currentPath === "/home" || currentPath === "/home/index")) {
      item.classList.add("active");
    } else if (route && currentPath.startsWith(`/${route}`)) {
      item.classList.add("active");
    } else {
      item.classList.remove("active");
    }
  });
}

/* ── DOM Init ── */
document.addEventListener("DOMContentLoaded", () => {
  SoundPlayer.init();
  highlightActiveMobileNav();

  // Attach track card play buttons
  document.querySelectorAll("[data-play-track]").forEach(btn => {
    btn.addEventListener("click", (e) => {
      e.preventDefault();
      e.stopPropagation();
      const track = {
        id: btn.dataset.trackId,
        title: btn.dataset.trackTitle,
        artist: btn.dataset.trackArtist,
        coverUrl: btn.dataset.trackCover || "/images/default-cover.jpg"
      };
      SoundPlayer.playTrack(track);
    });
  });

  // Like button toggles
  document.querySelectorAll("[data-like-track]").forEach(btn => {
    btn.addEventListener("click", async (e) => {
      e.preventDefault();
      const trackId = btn.dataset.likeTrack;
      try {
        const res = await fetch("/Music/Like", {
          method: "POST",
          headers: { "Content-Type": "application/x-www-form-urlencoded" },
          body: `trackId=${trackId}`
        });
        if (res.ok) {
          btn.classList.toggle("text-danger");
          Toast.success("Liked", "Track like status updated");
        } else {
          window.location.href = "/Account/Login";
        }
      } catch (err) {
        console.error(err);
      }
    });
  });

  // Plan select buttons
  document.querySelectorAll(".btn-plan-select").forEach(btn => {
    btn.addEventListener("click", function () {
      const planId = this.dataset.planId;
      if (!planId) return;
      window.location.href = `/Payment/Checkout?planId=${encodeURIComponent(planId)}`;
    });
  });
});
