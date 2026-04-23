document.addEventListener("DOMContentLoaded", () => {
  const planButtons = document.querySelectorAll(".btn-plan-select");

  for (const button of planButtons) {
    button.addEventListener("click", () => {
      const planId = button.dataset.planId;

      if (!planId) {
        return;
      }

      window.location.href = `/Payment/Checkout?planId=${encodeURIComponent(planId)}`;
    });
  }
});
