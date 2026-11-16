(() => {
  const ajaxForms = "form[data-ajax-filter='true']";
  const ajaxLinks = "[data-ajax-results] .pagination a, a[data-ajax-link='true']";

  async function replaceAjaxResults(url, targetSelector, shouldPushState = true) {
    const target = document.querySelector(targetSelector);
    if (!target) return;

    target.classList.add("ajax-loading");

    try {
      const response = await fetch(url, {
        headers: {
          "X-Requested-With": "XMLHttpRequest",
          "Cache-Control": "no-cache"
        }
      });

      if (!response.ok) {
        window.location.href = url;
        return;
      }

      const html = await response.text();
      const doc = new DOMParser().parseFromString(html, "text/html");
      const nextTarget = doc.querySelector(targetSelector);

      if (!nextTarget) {
        window.location.href = url;
        return;
      }

      target.replaceWith(nextTarget);

      if (shouldPushState) {
        window.history.pushState({ targetSelector }, "", url);
      }
    } catch {
      window.location.href = url;
    }
  }

  document.addEventListener("submit", (event) => {
    const form = event.target.closest(ajaxForms);
    if (!form) return;

    event.preventDefault();

    const formData = new FormData(form);
    const url = new URL(form.action, window.location.origin);

    for (const [key, value] of formData.entries()) {
      if (value) {
        url.searchParams.set(key, value);
      } else {
        url.searchParams.delete(key);
      }
    }

    url.searchParams.delete("page");

    replaceAjaxResults(url.toString(), form.dataset.ajaxTarget);
  });

  document.addEventListener("click", (event) => {
    const link = event.target.closest(ajaxLinks);
    if (!link || !link.href) return;

    const targetSelector = link.dataset.ajaxTarget || (link.closest("[data-ajax-results]")?.id
      ? `#${link.closest("[data-ajax-results]").id}`
      : "");

    if (!targetSelector) return;

    event.preventDefault();
    if (link.dataset.ajaxResetForm === "true") {
      const form = link.closest("form");
      form?.reset();
      form?.querySelectorAll("input[type='search'], input[type='text']").forEach((input) => {
        input.value = "";
      });
      form?.querySelectorAll("select").forEach((select) => {
        select.selectedIndex = 0;
      });
    }

    replaceAjaxResults(link.href, targetSelector);
  });

  window.addEventListener("popstate", (event) => {
    const targetSelector = event.state?.targetSelector;
    if (targetSelector) {
      replaceAjaxResults(window.location.href, targetSelector, false);
    } else {
      window.location.reload();
    }
  });
})();
