window.onload = () => {
    const input = document.querySelector('.file-input_image');

    const imageEl = document.querySelector('.profile-image-preview');

    const reader = new FileReader();

    reader.onload = e => {
        imageEl.setAttribute('src', e.target.result);
    };

    input.oninput = () => {
        if (!input.files || !input.length === 0) {
            return;
        }
        reader.readAsDataURL(input.files[0]);
    };
};