/* ShopingStore - اسکریپت فروشگاه */
(function () {
    'use strict';

    const token = () => {
        const input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    };

    const showToast = (message, type) => {
        const toast = document.createElement('div');
        toast.className = 'toast toast--' + (type || 'info');
        toast.textContent = message;
        const main = document.querySelector('.site-main');
        if (main) main.prepend(toast);
        setTimeout(() => toast.remove(), 5000);
    };

    // ------------------------------------------------------------ افزودن به سبد
    document.addEventListener('submit', async (event) => {
        const form = event.target;
        if (!form.matches('form[data-ajax="cart"]')) return;

        event.preventDefault();
        const button = form.querySelector('button[type="submit"]');
        if (button) button.disabled = true;

        try {
            const response = await fetch(form.action, {
                method: 'POST',
                body: new FormData(form),
                headers: { 'X-CSRF-TOKEN': token(), 'X-Requested-With': 'XMLHttpRequest' }
            });

            const data = await response.json();

            if (data.success) {
                showToast(data.message || 'محصول به سبد خرید اضافه شد.', 'success');
                const badge = document.querySelector('.header-action--cart .badge-count');
                if (badge && typeof data.cartCount !== 'undefined') badge.textContent = data.cartCount;
            } else {
                showToast(data.message || 'خطا در افزودن محصول.', 'error');
            }
        } catch (error) {
            showToast('ارتباط با سرور برقرار نشد.', 'error');
        } finally {
            if (button) button.disabled = false;
        }
    });

    // --------------------------------------------------------- علاقه‌مندی‌ها
    document.addEventListener('click', async (event) => {
        const button = event.target.closest('[data-wishlist]');
        if (!button) return;

        event.preventDefault();
        const formData = new FormData();
        formData.append('productId', button.dataset.wishlist);

        try {
            const response = await fetch('/api/wishlist/toggle', {
                method: 'POST',
                body: formData,
                headers: { 'X-CSRF-TOKEN': token(), 'X-Requested-With': 'XMLHttpRequest' }
            });

            const data = await response.json();

            if (!data.success) {
                showToast(data.message || 'برای افزودن به علاقه‌مندی‌ها وارد حساب خود شوید.', 'info');
                return;
            }

            button.classList.toggle('is-active', data.isFavorite);
            showToast(data.isFavorite ? 'به علاقه‌مندی‌ها اضافه شد.' : 'از علاقه‌مندی‌ها حذف شد.', 'success');
        } catch {
            showToast('ارتباط با سرور برقرار نشد.', 'error');
        }
    });

    // ------------------------------------------------------ گالری تصاویر محصول
    document.querySelectorAll('[data-gallery-thumb]').forEach((thumb) => {
        thumb.addEventListener('click', () => {
            const main = document.querySelector('[data-gallery-main]');
            if (!main) return;
            main.src = thumb.dataset.src || thumb.src;
            document.querySelectorAll('[data-gallery-thumb]').forEach((t) => t.classList.remove('is-active'));
            thumb.classList.add('is-active');
        });
    });

    // ----------------------------------------------------------- تب‌های محتوا
    document.querySelectorAll('[data-tab]').forEach((button) => {
        button.addEventListener('click', () => {
            const target = button.dataset.tab;
            document.querySelectorAll('[data-tab]').forEach((b) => b.classList.remove('is-active'));
            document.querySelectorAll('[data-tab-panel]').forEach((panel) => panel.classList.remove('is-active'));
            button.classList.add('is-active');
            const panel = document.querySelector(`[data-tab-panel="${target}"]`);
            if (panel) panel.classList.add('is-active');
        });
    });

    // ------------------------------------------- به‌روزرسانی تعداد در سبد خرید
    document.querySelectorAll('[data-cart-qty]').forEach((control) => {
        control.addEventListener('submit', async (event) => {
            event.preventDefault();
            const form = event.target;
            const response = await fetch(form.action, {
                method: 'POST',
                body: new FormData(form),
                headers: { 'X-CSRF-TOKEN': token(), 'X-Requested-With': 'XMLHttpRequest' }
            });

            if (response.redirected) {
                window.location.reload();
                return;
            }

            const data = await response.json().catch(() => null);
            if (data && data.success) showToast(data.message, 'success');
            window.location.reload();
        });
    });

    // -------------------------------------------------- پیشنهاد زنده جست‌وجو
    const searchInput = document.querySelector('.search-box input[type="search"]');
    const suggestList = document.querySelector('#search-suggest');
    let suggestTimer = null;

    if (searchInput && suggestList) {
        searchInput.addEventListener('input', () => {
            clearTimeout(suggestTimer);
            const term = searchInput.value.trim();
            if (term.length < 2) return;

            suggestTimer = setTimeout(async () => {
                try {
                    const response = await fetch('/api/products/suggest?term=' + encodeURIComponent(term));
                    const items = await response.json();

                    suggestList.innerHTML = '';
                    items.forEach((item) => {
                        const option = document.createElement('option');
                        option.value = item.name;
                        option.label = item.price;
                        suggestList.appendChild(option);
                    });
                } catch { /* نادیده بگیر */ }
            }, 250);
        });
    }

    setTimeout(() => document.querySelectorAll('.toast').forEach((t) => t.remove()), 6000);
})();
