function setIframeSrc(id, src) {
    const iframe = document.getElementById(id);
    if (iframe) {
        iframe.src = src;
    }
}
