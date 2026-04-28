// Función para obtener el token de reCAPTCHA

function getRecaptchaToken(siteKey, action) {
    return new Promise((resolve, reject) => {
        if (typeof grecaptcha === 'undefined') {
            reject('reCAPTCHA no está cargado');
            return;
        }

        grecaptcha.ready(() => {
            grecaptcha.execute(siteKey, { action: action })
                .then(token => {
                    if (!token) {
                        reject('Token vacío recibido');
                    } else {
                        resolve(token);
                    }
                })
                .catch(error => {
                    console.error('Error en reCAPTCHA:', error);
                    reject(error.toString());
                });
        });
    });
};

// Función para inicializar reCAPTCHA
function initializeRecaptcha(siteKey) {
    return new Promise((resolve) => {
        if (typeof grecaptcha !== 'undefined') {
            resolve();
            return;
        }

        const script = document.createElement('script');
        script.src = `https://www.google.com/recaptcha/api.js?render=${siteKey}`;
        script.async = true;
        script.defer = true;
        script.onload = resolve;
        script.onerror = () => console.error('Error cargando reCAPTCHA');
        document.body.appendChild(script);
    });
};
