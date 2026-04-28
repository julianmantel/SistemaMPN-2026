function setThemeForLoader(loadingScreen, observer) {
    let darkLightThemeValue = JSON.parse(localStorage.getItem('userPreferences'));

    let useLightTheme = darkLightThemeValue === 1;

    if (useLightTheme) {
        // Set background-color for light theme
        loadingScreen.style.backgroundColor = '#ffffff';
    } else {
        document.body.style.backgroundColor = 'rgba(50,51,61,1)';
        document.body.style.color = '#ffffff';
        //document.body.style.backgroundColor = 'var(--mud-palette-background)';
    }

    observer.disconnect();
}

// Observes for DOM changes to detect the loading-screen element.
const loadingScreenObserver = new MutationObserver((mutationsList, observer) => {
    for (let mutation of mutationsList) {
        if (mutation.type === 'childList') {
            loadingScreen = document.getElementById('app');

            if (loadingScreen) {
                setThemeForLoader(loadingScreen, observer);
                break;
            }
        }
    }
});

window.setBackgroundColor = (color) => {
    document.body.style.backgroundColor = color;
    document.body.style.color = "";
};

// Start observing the document body for changes in the DOM.
loadingScreenObserver.observe(document.body, { childList: true, subtree: true });

// Return prerender status
// For users we serve the wasm app without prerendering for bots we serve a prerendered wasm app
function getPreRender() {
    return document.documentElement.dataset.prerender;
}
