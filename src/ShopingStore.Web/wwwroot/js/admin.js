/* ShopingStore - اسکریپت پنل مدیریت */
(function () {
    'use strict';

    const addRow = (container, templateHtml) => {
        const wrapper = document.createElement('div');
        wrapper.innerHTML = templateHtml.trim();
        container.appendChild(wrapper.firstElementChild);
    };

    document.querySelectorAll('[data-add-spec]').forEach((button) => {
        button.addEventListener('click', () => {
            const container = document.querySelector('#specs-container');
            addRow(container, `<div class="repeat-row">
                <input name="SpecNames" placeholder="عنوان ویژگی (مثلاً رنگ)" />
                <input name="SpecValues" placeholder="مقدار (مثلاً مشکی)" />
                <button type="button" class="btn btn--ghost btn--sm" onclick="this.parentElement.remove()">حذف</button>
            </div>`);
        });
    });

    document.querySelectorAll('[data-add-variant]').forEach((button) => {
        button.addEventListener('click', () => {
            const container = document.querySelector('#variants-container');
            addRow(container, `<div class="repeat-row repeat-row--variant">
                <input name="VariantNames" placeholder="نام تنوع (مثلاً سایز)" />
                <input name="VariantOptions" placeholder="گزینه‌ها با کاما (S,M,L)" />
                <button type="button" class="btn btn--ghost btn--sm" onclick="this.parentElement.remove()">حذف</button>
            </div>`);
        });
    });

    // ---------------------------------------------------- پیش‌نمایش تصویر محصول
    const fileInput = document.querySelector('#image-upload');
    if (fileInput) {
        fileInput.addEventListener('change', async () => {
            const file = fileInput.files[0];
            if (!file) return;

            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            const formData = new FormData();
            formData.append('file', file);

            const response = await fetch(window.location.pathname + '?handler=UploadImage', {
                method: 'POST',
                body: formData,
                headers: { 'X-CSRF-TOKEN': token ? token.value : '' }
            });

            const data = await response.json();

            if (data.success) {
                const input = document.querySelector('#MainImageUrl');
                if (input) input.value = data.url;
                const preview = document.querySelector('#image-preview');
                if (preview) {
                    const img = document.createElement('img');
                    img.src = data.url;
                    preview.prepend(img);
                }
            } else {
                alert(data.message || 'خطا در بارگذاری تصویر.');
            }
        });
    }

    // ------------------------------------------------------- تأیید حذف رکوردها
    document.querySelectorAll('form[data-confirm]').forEach((form) => {
        form.addEventListener('submit', (event) => {
            if (!confirm(form.dataset.confirm)) event.preventDefault();
        });
    });
})();
