/* ── Toast Notifications ── */

const Toast = {
  icons: { success: "\u2713", error: "\u2717", info: "\u2139", warning: "\u26A0" },

  show(title, message, type, duration) {
    type = type || "info";
    duration = duration || 5000;
    const container = document.getElementById("toast-container");
    if (!container) return;

    const el = document.createElement("div");
    el.className = "toast-notification toast-" + type;
    el.innerHTML =
      '<span class="toast-icon">' + (this.icons[type] || "") + "</span>" +
      '<div class="toast-content">' +
        '<div class="toast-title">' + this.escape(title) + "</div>" +
        (message ? '<div class="toast-message">' + this.escape(message) + "</div>" : "") +
      "</div>" +
      '<button class="toast-dismiss" onclick="this.closest(\'.toast-notification\').classList.add(\'removing\');setTimeout(function(){this.closest(\'.toast-notification\').remove()}.bind(this),300)" aria-label="Dismiss">\u00D7</button>';
    container.appendChild(el);

    if (duration > 0) {
      setTimeout(function () {
        el.classList.add("removing");
        setTimeout(function () { el.remove(); }, 300);
      }, duration);
    }
  },

  success(title, message, duration) { this.show(title, message, "success", duration); },
  error(title, message, duration) { this.show(title, message, "error", duration); },
  info(title, message, duration) { this.show(title, message, "info", duration); },
  warning(title, message, duration) { this.show(title, message, "warning", duration); },

  escape(str) {
    if (!str) return "";
    var div = document.createElement("div");
    div.appendChild(document.createTextNode(str));
    return div.innerHTML;
  }
};

/* ── Loading Overlay ── */

const Loading = {
  show() {
    var el = document.getElementById("loading-overlay");
    if (el) el.classList.add("active");
  },

  hide() {
    var el = document.getElementById("loading-overlay");
    if (el) el.classList.remove("active");
  }
};

/* ── Plan Select Buttons ── */

document.addEventListener("DOMContentLoaded", function () {
  var buttons = document.querySelectorAll(".btn-plan-select");
  for (var i = 0; i < buttons.length; i++) {
    buttons[i].addEventListener("click", function () {
      var planId = this.dataset.planId;
      if (!planId) return;
      window.location.href = "/Payment/Checkout?planId=" + encodeURIComponent(planId);
    });
  }
});
