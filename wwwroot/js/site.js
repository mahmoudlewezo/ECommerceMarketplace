(function () {
    document.querySelectorAll(".custom-alert").forEach(function (alertEl) {
        window.setTimeout(function () {
            alertEl.classList.add("d-none");
        }, 6000);
    });

    document.querySelectorAll(".product-media img, .pdp-media img").forEach(function (img) {
        img.addEventListener("error", function () {
            var wrap = img.parentElement;
            img.remove();
            if (wrap && !wrap.querySelector(".product-media-fallback")) {
                var fallback = document.createElement("div");
                fallback.className = "product-media-fallback";
                fallback.textContent = "Image unavailable";
                wrap.appendChild(fallback);
            }
        });
    });
})();
