// ==========================================
// VISOR Y DESCARGA DE DOCUMENTOS
// ==========================================

/**
 * Crea un Blob URL desde un array de bytes
 * @param {Uint8Array} byteArray - Array de bytes del archivo
 * @param {string} contentType - Tipo MIME del archivo (ej: "application/pdf")
 * @returns {string} URL del Blob creado
 */
window.createBlobUrl = function (byteArray, contentType) {
    try {
        if (!byteArray || byteArray.length === 0) {
            console.error("El array de bytes está vacío");
            return null;
        }

        const blob = new Blob([new Uint8Array(byteArray)], { type: contentType });
        const blobUrl = URL.createObjectURL(blob);

        console.log(`Blob URL creado exitosamente: ${blobUrl}`);
        return blobUrl;
    } catch (error) {
        console.error("Error al crear Blob URL:", error);
        return null;
    }
};

/**
 * Descarga un archivo desde una Blob URL
 * @param {string} blobUrl - URL del blob a descargar
 * @param {string} fileName - Nombre del archivo para la descarga
 */
window.downloadBlob = function (blobUrl, fileName) {
    try {
        if (!blobUrl) {
            console.error("La URL del blob está vacía");
            return;
        }

        const link = document.createElement('a');
        link.href = blobUrl;
        link.download = fileName || 'documento.pdf';
        link.style.display = 'none';

        document.body.appendChild(link);
        link.click();

        // Cleanup
        setTimeout(() => {
            document.body.removeChild(link);
        }, 100);

        console.log(`Descarga iniciada: ${fileName}`);
    } catch (error) {
        console.error("Error al descargar blob:", error);
    }
};

/**
 * Abre un documento en una nueva pestaña
 * @param {string} url - URL del documento a abrir
 */
window.abrirDocumentoNuevaPestaña = function (url) {
    try {
        if (!url) {
            console.error("La URL está vacía");
            return;
        }

        const nuevaVentana = window.open(url, '_blank');

        if (!nuevaVentana) {
            console.warn("No se pudo abrir la ventana. Puede estar bloqueada por el navegador.");
            // Fallback: intentar con link
            const link = document.createElement('a');
            link.href = url;
            link.target = '_blank';
            link.click();
        }

        console.log(`Documento abierto en nueva pestaña: ${url}`);
    } catch (error) {
        console.error("Error al abrir documento:", error);
    }
};

/**
 * Libera un Blob URL de la memoria
 * @param {string} blobUrl - URL del blob a liberar
 */
window.liberarBlobUrl = function (blobUrl) {
    try {
        if (blobUrl && blobUrl.startsWith('blob:')) {
            URL.revokeObjectURL(blobUrl);
            console.log(`Blob URL liberado: ${blobUrl}`);
        }
    } catch (error) {
        console.error("Error al liberar Blob URL:", error);
    }
};

/**
 * Descarga un documento directamente desde una URL del servidor
 * @param {string} url - URL del endpoint del servidor
 * @param {string} fileName - Nombre del archivo para la descarga
 */
window.descargarDocumentoDesdeUrl = async function (url, fileName) {
    try {
        const response = await fetch(url);

        if (!response.ok) {
            console.error(`Error en la descarga: ${response.status} ${response.statusText}`);
            return false;
        }

        const blob = await response.blob();
        const blobUrl = URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = blobUrl;
        link.download = fileName || 'documento.pdf';
        link.style.display = 'none';

        document.body.appendChild(link);
        link.click();

        // Cleanup
        setTimeout(() => {
            document.body.removeChild(link);
            URL.revokeObjectURL(blobUrl);
        }, 100);

        console.log(`Documento descargado: ${fileName}`);
        return true;
    } catch (error) {
        console.error("Error al descargar documento desde URL:", error);
        return false;
    }
};

/**
 * Convierte un array de bytes a Base64
 * @param {Uint8Array} byteArray - Array de bytes
 * @returns {string} String en Base64
 */
window.bytesToBase64 = function (byteArray) {
    try {
        let binary = '';
        const bytes = new Uint8Array(byteArray);
        const len = bytes.byteLength;

        for (let i = 0; i < len; i++) {
            binary += String.fromCharCode(bytes[i]);
        }

        return window.btoa(binary);
    } catch (error) {
        console.error("Error al convertir bytes a Base64:", error);
        return null;
    }
};

/**
 * Verifica si un Blob URL es válido
 * @param {string} blobUrl - URL del blob a verificar
 * @returns {boolean} true si es válido
 */
window.esBlobUrlValido = function (blobUrl) {
    return blobUrl &&
        typeof blobUrl === 'string' &&
        blobUrl.startsWith('blob:');
};
