function chequearJSPDF() {
    if (!window.jspdfLoaded) {
        const script = document.createElement('script');
        script.src = 'https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js';
        script.onload = () => {
            window.jspdfLoaded = true;
        };
        document.head.appendChild(script);
    }
}

function descargarPDF(tipo, pdfURL, num) {
    const link = document.createElement('a');
    link.href = `${pdfURL}`;

    switch (tipo) {
        case "DetalleCaja":
            link.download = `detalle-caja-${num}.pdf`;
            break;
        case "Diezmo":
            link.download = `planilla-diezmos-${num}.pdf`;
            break;
        case "ReciboGeneral":
            link.download = `recibo-general-${num}.pdf`;
            break;
        case "ReciboOV":
        link.download = `recibo-ov-${num}.pdf`;
        break;
    }

    link.click();
}

function agregarMarcaAgua(doc) {
    const w = doc.internal.pageSize.getWidth();
    const h = doc.internal.pageSize.getHeight();

    // Definir estado gráfico con alpha
    const gState = doc.GState({ opacity: 0.2, blendMode: 'Normal' });
    doc.setGState(gState);

    doc.setTextColor(255, 0, 0); // rojo puro
    doc.setFontSize(60);
    doc.setFont("helvetica", "bold");
    doc.text("NO VÁLIDO", w / 2, h / 2, { align: "center", angle: 30 });

    // Restaurar estado gráfico
    doc.setGState(new doc.GState({ opacity: 1 }));
}

// Helper para cargar imagen sin bloquear
function cargarImagen(url) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.crossOrigin = "Anonymous"; // Evita problemas de CORS
        img.onload = () => resolve(img);
        img.onerror = () => reject(new Error("No se pudo cargar la imagen en: " + url));
        img.src = url;
    });
}

// Helper para liberar el hilo de ejecución
const permitirRespirar = () => new Promise(resolve => setTimeout(resolve, 0));

async function generarDetalleCaja(data, preview = true) {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF({
        orientation: 'p',
        unit: 'mm',
        format: 'a5',
        putOnlyUsedFonts: true
    });

    const margin = 20;
    let y = margin;

    // 1. Cargar Logo
    try {
        const img = await cargarImagen("../images/logo.png");
        doc.addImage(img, "PNG", 10, y - 10, 25, 30.9);
    } catch (e) {
        console.warn("Logo no encontrado, continuando sin imagen.");
    }

    await permitirRespirar();

    // 2. Encabezado
    let headerTextX = 105;
    doc.setFontSize(9);
    doc.setFont('helvetica', 'bold');
    doc.text('IGLESIA EVANGÉLICA PENTECOSTAL', headerTextX, y, { align: 'center' });
    y += 4;
    doc.text('MINISTERIO PROFÉTICO A LAS NACIONES', headerTextX, y, { align: 'center' });
    y += 4;
    doc.setFontSize(8);
    doc.setFont(undefined, 'normal');
    doc.text('Personería Jurídica N° 419', headerTextX, y, { align: 'center' });
    y += 4;
    doc.text('Fichero de culto N° 106', headerTextX, y, { align: 'center' });
    y += 4;
    doc.text('Cuit: 30-69033186-8', headerTextX, y, { align: 'center' });
    y += 4;
    doc.setFont(undefined, 'bold');
    doc.text('IVA EXENTO', headerTextX, y, { align: 'center' });
    y += 15;

    await permitirRespirar();

    // 3. Datos Principales
    const fsDatos = 12;
    doc.setFont("helvetica", "normal");
    doc.setFontSize(fsDatos);
    doc.text("Fecha:", margin, y);
    doc.setFont("courier", "bold");
    doc.text(" " + (data.fecha || ""), margin + 12, y);

    doc.setFont("helvetica", "normal");
    doc.text("Reunion:", 90, y);
    doc.setFont("courier", "bold");
    doc.text(" " + (data.reunion || ""), 105, y);
    y += 8;

    doc.setFont("helvetica", 'bold');
    doc.text('DETALLE DE CAJA', margin, y);
    doc.setFont("helvetica", 'normal');
    doc.text("ALF-", 90, y);
    doc.setFont("courier", "bold");
    doc.text(" " + (data.numero || ""), 97, y);
    y += 5;

    // 4. Tabla de Denominaciones
    const tableStartY = y;
    const tableWidth = 108;
    const rowHeight = 9;
    const activos = data.denominaciones.filter(d => d.cantidad >= 0);

    doc.rect(margin, tableStartY, tableWidth, activos.length * rowHeight);

    let totalPesos = 0;
    for (let i = 0; i < activos.length; i++) {
        const item = activos[i];
        const rowY = tableStartY + 5 + (i * rowHeight);
        const subtotal = item.denominacion * item.cantidad;
        totalPesos += subtotal;

        doc.setFont("helvetica", 'normal');
        doc.setFontSize(12);
        doc.text('$', margin + 2, rowY);
        doc.text(item.denominacion.toLocaleString('es-AR'), margin + 20, rowY, { align: 'right' });
        doc.text('X', margin + 25, rowY);
        doc.line(margin + 30, rowY + 1, margin + 50, rowY + 1);
        doc.text(item.cantidad.toString(), margin + 40, rowY, { align: 'center' });
        doc.text('=', margin + 55, rowY);
        doc.text('$', margin + 63, rowY);
        doc.line(margin + 70, rowY + 1, margin + 105, rowY + 1);

        if (subtotal > 0) {
            doc.text(subtotal.toLocaleString('es-AR', { minimumFractionDigits: 2 }), margin + 87.5, rowY, { align: 'center' });
        }

        // Si la tabla es muy larga, liberamos el hilo cada 5 filas
        if (i % 5 === 0) await permitirRespirar();
    }

    y = tableStartY + ((activos.length + 1) * rowHeight) - 3;

    // 5. Totales
    doc.rect(margin, y - 6, tableWidth, 9);
    doc.setFont("helvetica", 'bold').setFontSize(15);
    doc.text('TOTAL $:', 50, y);
    doc.setFont("courier", "bold").text(totalPesos.toLocaleString('es-AR', { minimumFractionDigits: 2 }), 75, y);

    y += 9;
    doc.rect(margin, y - 6, tableWidth, 8);
    doc.setFont("helvetica", 'bold').text('TOTAL U$D:', 50, y);
    doc.setFont("courier", 'bold').text((data.totalUSD || 0).toLocaleString('es-AR', { minimumFractionDigits: 2 }), 82, y);

    await permitirRespirar();

    // 6. Observaciones
    y += 10;
    doc.setFont("helvetica", "normal").setFontSize(12);
    doc.text("Observaciones:", margin, y);

    const obsTexto = "Observaciones: " + (data.obs || "");
    const lineas = doc.splitTextToSize(obsTexto, 117);

    let isFirstLine = true;
    lineas.forEach(linea => {
        if (isFirstLine) {
            doc.setFont("courier", "normal").text(linea.slice(14), margin + 28, y);
            isFirstLine = false;
        } else {
            doc.text(linea, margin, y);
        }
        y += 5;
    });

    // 7. Firmas
    y = 180;
    doc.line(margin, y + 10, margin + 50, y + 10);
    doc.line(margin + 58, y + 10, margin + 108, y + 10);
    doc.setFont("helvetica", 'normal').setFontSize(9);
    doc.text('Firma y Aclaración de Tesorero', margin + 25, y + 14, { align: 'center' });
    doc.text('Firma y Aclaración de Supervisor', margin + 83, y + 14, { align: 'center' });

    // Marco exterior
    const pw = doc.internal.pageSize.getWidth();
    const ph = doc.internal.pageSize.getHeight();
    doc.setLineWidth(0.1).rect(7, 7, pw - 14, ph - 14);

    if (preview) {
        if (typeof agregarMarcaAgua === 'function') agregarMarcaAgua(doc);
    }

    // 8. Finalización (Blob)
    await permitirRespirar();
    const pdfBlob = doc.output('blob');
    return URL.createObjectURL(pdfBlob);
}

async function agregarEncabezadoPD(doc, nro, fecha, tesorero, supervisor) {
    let y = 15;
    let margin = y;
    
    try {
        const img = await cargarImagen("../images/logo.png");
        doc.addImage(img, "PNG", 10, y - 10, 25, 30.9);
    } catch (e) {
        console.warn("Logo no encontrado, continuando sin imagen.");
    }
    
    await permitirRespirar();

    y += 9;

    // Encabezado
    doc.setFontSize(10);
    doc.setFont("helvetica", "normal");
    doc.text("IGLESIA EVANGÉLICA PENTECOSTAL", 105, 15, { align: "center" });
    doc.text("MINISTERIO PROFÉTICO A LAS NACIONES", 105, 20, { align: "center" });

    // Nro y Fecha
    doc.setFontSize(11);
    doc.text("Nro:", 185, 15, { align: "right" });

    doc.setFont("courier", "bold"); doc.setFontSize(12);
    doc.text(nro, 195, 15, { align: "right" });

    doc.setFont("Helvetica", "bold"); doc.setFontSize(11);
    doc.text("Fecha:", 169, 22, { align: "right" });
    doc.setFont("courier", "bold"); doc.setFontSize(12);
    doc.text(fecha, 195, 22, { align: "right" });

    doc.setFont("Helvetica", "bold");
    // Título
    y = 30;
    doc.setFontSize(16);
    doc.text("Planilla Diezmo", 105, y, { align: "center" });

    y = 50;

    doc.setFontSize(11);
    doc.setFont("courier", "normal");
    doc.text(tesorero, 55, y - 1, { align: "center" });

    doc.text(supervisor, 152, y - 1, { align: "center" });

    doc.setFontSize(9);
    doc.setFont("helvetica", "normal");
    doc.line(margin, y, 100, y);
    doc.text("Tesorero", 55, y + 5, { align: "center" });
    doc.line(110, y, 195, y);
    doc.text("Supervisor", 152, y + 5, { align: "center" });
}

async function generarPlanillaDiezmos(data, preview = true) {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF();
    const margin = 15;
    const pageWidth = 210;
    const pageHeight = 297;
    let y = margin;

    let limiteFilas = 25;

    const col1Width = 15;
    const col2Width = 125;
    const col3Width = 40;
    const rowHeight = 7;

    const totalPages = Math.ceil(data.registros.length / limiteFilas);
    console.log("Total Pages:", totalPages);

    for (let i = 0; i < totalPages; i++) {
        y = 58;
        if (i > 0) {
            doc.addPage();
        }

        await agregarEncabezadoPD(doc, data.numero, data.fecha, data.nombreTesorero, data.nombreSupervisor); //Problemas

        doc.setFillColor(200, 200, 200);
        doc.rect(margin, y, col1Width + col2Width + col3Width, 8, "F");
        doc.setFontSize(9);
        doc.setFont("helvetica", "bold");
        doc.text("N°", margin + 7, y + 5, { align: "center" });
        doc.text("Apellido, Nombre", margin + col1Width + 62, y + 5, { align: "center" });
        doc.text("Observación", margin + col1Width + col2Width + 20, y + 5, { align: "center" });
        doc.rect(margin, y, col1Width, rowHeight + 1);
        doc.rect(margin + col1Width, y, col2Width, rowHeight + 1);
        doc.rect(margin + col1Width + col2Width, y, col3Width, rowHeight + 1);

        let index = 0;
        y += 8;

        for (let j = 0; j < limiteFilas; j++) {
            index = i * limiteFilas + j;
            if (index >= data.registros.length) {
                break;
            }
            // Filas de datos
            doc.setFontSize(10);
            doc.rect(margin, y, col1Width, rowHeight);
            doc.rect(margin + col1Width, y, col2Width, rowHeight);
            doc.rect(margin + col1Width + col2Width, y, col3Width, rowHeight);

            doc.setFont("courier", "normal");
            doc.setFontSize(11);

            doc.text(data.registros[index].numero.toString(), margin + 7, y + 4.5, { align: "center" });
            doc.text(data.registros[index].nombreApellido, margin + col1Width + 2, y + 4.5);
            doc.text(data.registros[index].observacion, margin + col1Width + col2Width + 20, y + 4.5, { align: "center" });
            y += 7;
        }


    }

    y += 10;

    let textoEntrega = "Los sobres de diezmos ingresaron en el Alfolí, no se abrieron ni se controló el dinero que hay dentro de ellos. Solamente se numeraron y registraron sus nombres en esta planilla. Posteriormente se entregaron a " + data.textoEntrega + " en la misma condición que los recibimos.";

    // Texto de entrega
    doc.setFont("helvetica", "normal");
    const textLines = doc.splitTextToSize(textoEntrega, pageWidth - margin * 2);
    textLines.forEach(line => {
        if (y > pageHeight - 30) {
            doc.addPage();
            y = margin;
        }
        doc.text(line, margin, y);
        y += 5;
    });

    // Firmas inferiores
    y += 15;
    doc.line(margin, y, 90, y);
    doc.text("Firma Tesorero", 52, y + 5, { align: "center" });
    doc.line(120, y, pageWidth - margin, y);
    doc.text("Firma Supervisor", 157, y + 5, { align: "center" });

    if (preview) {
        agregarMarcaAgua(doc);
    }

    const pdfBlob = doc.output('blob');
    const pdfUrl = URL.createObjectURL(pdfBlob);

    return pdfUrl;
}

function generarReciboGeneral(data, preview = true) {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF({
        orientation: 'l',
        unit: 'mm',
        format: 'a5',
        putOnlyUsedFonts: true
    });
    const margin = 10;
    let y = margin;
    const fsDatos = 12;

    const img = new Image();
    img.src = "../images/logo.png";

    if (data.automatico == true) {
        data.montoLetras = numeroALetras(data.montoNum, "PESOS");
    }

    doc.addImage(img, "PNG", 10, y , 23, 28.444444444444443);
    y += 9;

    //Membrete
    let headerTextX = 75;
    doc.setFontSize(9);
    doc.setFont('helvetica', 'bold');
    doc.text('IGLESIA EVANGÉLICA PENTECOSTAL', headerTextX, y, { align: 'center' });
    y += 4;
    doc.setFontSize(9);
    doc.text('MINISTERIO PROFÉTICO A LAS NACIONES', headerTextX, y, { align: 'center' });
    y += 4;
    doc.setFontSize(9);
    doc.setFont(undefined, 'normal');
    doc.text('Personería Jurídica N° 419 - Fichero de culto N° 106', headerTextX, y, { align: 'center' });
    y += 4;
    doc.text('Cuit: 30-69033186-8 - IVA EXENTO', headerTextX, y, { align: 'center' });
    y += 4;
    doc.setFont(undefined, 'bold');
    doc.text('TESORERIA GENERAL', headerTextX, y, { align: 'center' });
    y += 15;

    //Datos Encabezado
    y = 22;
    doc.setFontSize(fsDatos);
    doc.setFont("helvetica", 'normal');
    doc.text("Nro: ALF-", 175.3, y, { align: "right" });
    doc.setFont("courier", "bold");
    doc.setFontSize(fsDatos + 1);
    doc.text(" " + data.numero, 173, y, { align: "left" });
    y += 8;
    doc.setFont("helvetica", 'normal');
    doc.text("Fecha:", 171, y, { align: "right" });
    doc.setFont("courier", "bold");
    doc.setFontSize(fsDatos + 1);
    doc.text(" " + data.fecha, 200, y, { align: "right" });

    //Numeros Letras
    const maxWidth = 140;
    y = 60;
    doc.setFont("helvetica", "normal");
    doc.setFontSize(13);
    doc.text("Recibí de Tesorería General la cantidad de pesos:", margin, y);

    //max 130
    //data.montoLetras = "Lorem m Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy tex standard dummy tex standard dummy tex standard dummy tex standar d dummy text d dummy text d dummy textd dummy text d dummy text";
    data.montoLetras = (data.montoLetras).slice(0, 130);
    y += 5;
    const letras = doc.splitTextToSize(data.montoLetras, maxWidth);
    doc.setFontSize(14);
    doc.setFont("courier", "normal");
    letras.forEach(linea => {
        doc.text(linea, margin, y);
        y += 5;
    });

    //Concepto
    y += 6;
    doc.setFont("helvetica", "normal");
    doc.setFontSize(13);
    doc.text("En concepto de", margin, y);

    let flag = true;
    //max 250
    //data.concepto = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy tex standard dummy tex standard dummy tex standard dummy tex standar d dummy text d dummy text d dummy textd dummy text d dummy text";
    data.concepto = (data.concepto).slice(0, 250);

    data.concepto = "En concepto de " + data.concepto;

    const concep = doc.splitTextToSize(data.concepto, maxWidth);
    doc.setFontSize(14);
    doc.setFont("courier", "normal");
    concep.forEach(linea => {
        if (flag) {
            linea = linea.slice(15);
            doc.text(linea, margin + 33, y);
            flag = false;
        }
        else {
            doc.text(linea, margin, y);
        }
        y += 5;
    });


    //$$$
    //$$$
    y += 6;
    doc.setFont("helvetica", "normal");
    doc.setFontSize(13);
    doc.text("Son $", margin, y);
    doc.setFontSize(14);
    doc.setFont("courier", "bold");

    // Convertir a número antes de formatear
    const montoFormateado = parseFloat(data.montoNum).toLocaleString('es-AR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    doc.text(montoFormateado, margin + 13, y);
    doc.setFontSize(9);
    doc.setFont("helvetica", 'normal');

    y += 5;
    doc.line(180, y, 120, y);
    y += 4;
    doc.text("Firma del que recibe", 150, y, { align: "center" });

    y += 15;
    doc.line(180, y, 120, y);
    y += 4;
    doc.text("Aclaración y DNI", 150, y, { align: "center" });

    if (preview) {
        agregarMarcaAgua(doc);
    }

    const pdfBlob = doc.output('blob');
    const pdfUrl = URL.createObjectURL(pdfBlob);

    return pdfUrl;
}

function generarReciboOV(data, preview = true) {
    const { jsPDF } = window.jspdf;
    const doc = new jsPDF({
        orientation: 'l',
        unit: 'mm',
        format: 'a5',
        putOnlyUsedFonts: true
    });
    const margin = 10;
    let y = margin;
    const fsDatos = 12;

    const img = new Image();
    img.src = "../images/logo.png";

    doc.addImage(img, "PNG", 10, y, 23, 28.444444444444443);


    y += 9;

    //Membrete
    let headerTextX = 75;
    doc.setFontSize(9);
    doc.setFont('helvetica', 'bold');
    doc.text('IGLESIA EVANGÉLICA PENTECOSTAL', headerTextX, y, { align: 'center' });
    y += 4;
    doc.setFontSize(9);
    doc.text('MINISTERIO PROFÉTICO A LAS NACIONES', headerTextX, y, { align: 'center' });
    y += 4;
    doc.setFontSize(9);
    doc.setFont(undefined, 'normal');
    doc.text('Personería Jurídica N° 419 - Fichero de culto N° 106', headerTextX, y, { align: 'center' });
    y += 4;
    doc.text('Cuit: 30-69033186-8 - IVA EXENTO', headerTextX, y, { align: 'center' });
    y += 4;
    doc.setFont(undefined, 'bold');
    doc.text('TESORERIA GENERAL', headerTextX, y, { align: 'center' });
    y += 15;

    //Datos Encabezado
    y = 22;
    doc.setFontSize(fsDatos);
    doc.setFont("helvetica", 'normal');
    doc.text("Nro: AUD-", 175.3, y, { align: "right" });
    doc.setFont("courier", "bold");
    doc.setFontSize(fsDatos + 1);
    doc.text(" " + data.numero, 173, y, { align: "left" });
    y += 8;
    doc.setFont("helvetica", 'normal');
    doc.text("Fecha:", 170, y, { align: "right" });
    doc.setFont("courier", "bold");
    doc.setFontSize(fsDatos + 1);
    doc.text(" " + data.fecha, 199, y, { align: "right" });

    y += 15;
    doc.line(margin, y, 200, y);
    //Titulo
    y += 10;
    doc.setFont("helvetica", "bold");
    doc.setFontSize(14);
    doc.text("OFRENDA VOLUNTARIA", 105, y, { align: "center" });

    //Datos
    y += 10;
    const maxWidth = 140;
    doc.setFont("helvetica", "normal");
    doc.setFontSize(13);
    doc.text("Grupo/Celula:", margin, y);
    doc.setFont("courier", "normal");
    doc.setFontSize(14);
    doc.text(data.grupo, margin + 30, y); //limite 55 caracteres

    y += 15;
    doc.setFont("helvetica", "normal");
    doc.setFontSize(13);
    doc.text("Ofrenda Voluntariamente en:", margin, y);



    y += 8;
    doc.text("Pesos: $ ", margin, y);
    doc.setFont("courier", "normal");
    doc.setFontSize(14);

    let montoFormateado = parseFloat(data.pesos).toLocaleString('es-AR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    doc.text(montoFormateado, margin + 18, y);

    montoFormateado = parseFloat(data.dolares).toLocaleString('es-AR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    const cero = parseFloat(0).toLocaleString('es-AR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });

    if (montoFormateado > cero) {
        doc.setFont("helvetica", "normal");
        doc.setFontSize(13);
        doc.text("Dólares: U$D ", 100, y);
        doc.setFont("courier", "normal");
        doc.setFontSize(14);
        doc.text(montoFormateado, 129, y);
    }

    y += 10;
    doc.setFont("helvetica", "normal");
    doc.setFontSize(13);
    doc.text("Otros:", margin, y);
    doc.setFont("courier", "normal");
    doc.setFontSize(14);
    doc.text(data.otros, margin + 13, y); //max 60 caracteres

    y += 13;
    doc.setFont("helvetica", "normal");
    doc.setFontSize(13);
    doc.text("Para:", margin, y);

    doc.setFont("courier", "normal");
    doc.setFontSize(14);
    doc.text(data.para, margin + 12, y); //max 60 caracteres

    // FIRMAS
    y += 16;
    const signatureWidth = 90;
    const signatureGap = 10;
    doc.setFontSize(9);
    doc.setFont("helvetica", 'normal');

    // Supervisor
    doc.setFont("courier", 'normal');
    doc.line(margin, y + 10, margin + signatureWidth, y + 10);
    doc.line(margin + signatureWidth + signatureGap, y + 10, margin + signatureWidth + signatureGap + signatureWidth, y + 10);
    doc.setFont("helvetica", 'normal');
    doc.text('Firma y Aclaración del tesorero que recibe', margin + (signatureWidth / 2), y + 14, { align: 'center' });
    doc.text('Firma y Aclaración del supervisor que recibe', margin + signatureWidth + signatureGap + (signatureWidth / 2), y + 14, { align: 'center' });

    if (preview) {
        agregarMarcaAgua(doc);
    }

    // Generar Blob y URL
    const pdfBlob = doc.output('blob');
    const pdfUrl = URL.createObjectURL(pdfBlob);

    return pdfUrl;
}

window.obtenerBytesDelBlob = async function (blobUrl) {
    try {
        const response = await fetch(blobUrl);
        const blob = await response.blob();

        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onloadend = () => {
                const base64 = reader.result.split(',')[1]; // Obtener solo la parte base64
                resolve(base64);
            };
            reader.onerror = reject;
            reader.readAsDataURL(blob);
        });
    } catch (error) {
        console.error("Error al obtener los bytes del Blob:", error);
        return null;
    }
}


