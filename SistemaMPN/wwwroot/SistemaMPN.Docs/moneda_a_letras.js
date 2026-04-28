const centavosFormatter = new Intl.NumberFormat('en-US', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
});

/**
 * Función para convertir un número a letras, con centavos (ideal para representar dinero). Fuente: https://gist.github.com/alfchee/e563340276f89b22042a
 * 
 * @param {float} cantidad - La cantidad a convertir en letras.
 * @param {string} moneda - Moneda opcional para desplegarse en el texto si es que se especifica una. Ej: "PESOS", "DÓLARES" 
 * 
 * NumeroALetras
 * The MIT License (MIT)
 * 
 * Copyright (c) 2015 Luis Alfredo Chee 
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * @author Rodolfo Carmona
 * @contributor Jean (jpbadoino@gmail.com)
 * 
 */
function numeroALetras(cantidad, moneda) {

    var numero = 0;
    cantidad = filterNum(cantidad);
    cantidad = parseFloat(cantidad);

    if ( cantidad == "0") {
        return ("CERO " + moneda);
    } else {
        var cantidadConCentavosExplicitos = centavosFormatter.format(parseFloat(cantidad)).toString().split(".");
        var ent = cantidad.toString().split(".");
        var arreglo = separar_split(ent[0]);
        var longitud = arreglo.length;

        switch (longitud) {
            case 1:
                numero = unidades(arreglo[0]);
                break;
            case 2:
                numero = decenas(arreglo[0], arreglo[1]);
                break;
            case 3:
                numero = centenas(arreglo[0], arreglo[1], arreglo[2]);
                break;
            case 4:
                numero = unidadesdemillar(arreglo[0], arreglo[1], arreglo[2], arreglo[3]);
                break;
            case 5:
                numero = decenasdemillar(arreglo[0], arreglo[1], arreglo[2], arreglo[3], arreglo[4]);
                break;
            case 6:
                numero = centenasdemillar(arreglo[0], arreglo[1], arreglo[2], arreglo[3], arreglo[4], arreglo[5]);
                break;
            case 7:
                numero = unidadesdemillon(arreglo[0], arreglo[1], arreglo[2], arreglo[3], arreglo[4], arreglo[5], arreglo[6]);
                break;
            case 8:
                numero = decenasdemillon(arreglo[0], arreglo[1], arreglo[2], arreglo[3], arreglo[4], arreglo[5], arreglo[6], arreglo[7]);
                break;
            case 9:
                numero = centenasdemillon(arreglo[0], arreglo[1], arreglo[2], arreglo[3], arreglo[4], arreglo[5], arreglo[6], arreglo[7], arreglo[8]);
                break;
            default:
                numero = "______________________________________________________________________";
                break;
        }

        cantidadConCentavosExplicitos[1] = isNaN(cantidadConCentavosExplicitos[1]) ? '00' : cantidadConCentavosExplicitos[1];

        if (cantidad == "1000000" && numero == "UN MILLÓN MIL ") {
            numero = "UN MILLÓN ";
        }

        var divisibleEntreUnMillon = parseInt(cantidad) % 1000000;

        if (divisibleEntreUnMillon == 0) {
            numero = numero.replace("MILLONES MIL", "MILLONES");
        }

        if (moneda) {
            if (cantidad == "1000000" && numero == "UN MILLÓN ") {
                numero = "UN MILLÓN DE ";
            }
            if (divisibleEntreUnMillon == 0 && parseInt(cantidad) > 1000000) {
                numero = numero.replace("MILLONES", "MILLONES DE ");
            }
            return numero + "" + moneda;
        } else {
            return numero + cantidadConCentavosExplicitos[1] + "/100";
        }
    }
}

const filterNum = (str) => {
    const numericalChar = new Set([".", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"]);
    str = str.split("").filter(char => numericalChar.has(char)).join("");
    return str;
}

function unidades(unidad) {
    var unidades = Array('UN ', 'DOS ', 'TRES ', 'CUATRO ', 'CINCO ', 'SEIS ', 'SIETE ', 'OCHO ', 'NUEVE ');


    return unidades[unidad - 1];
}

function decenas(decena, unidad) {
    var diez = Array('ONCE ', 'DOCE ', 'TRECE ', 'CATORCE ', 'QUINCE ', 'DIECISEIS ', 'DIECISIETE ', 'DIECIOCHO ', 'DIECINUEVE ');
    var decenas = Array('DIEZ ', 'VEINTE ', 'TREINTA ', 'CUARENTA ', 'CINCUENTA ', 'SESENTA ', 'SETENTA ', 'OCHENTA ', 'NOVENTA ');
 
    if (decena == 0 && unidad == 0) {
        return "";
    }

    if (decena == 0 && unidad > 0) {
        return unidades(unidad);
    }

    if (decena == 1) {
        if (unidad == 0) {
            return decenas[decena - 1];
        } else {
            return diez[unidad - 1];
        }
    } else if (decena == 2) {
        if (unidad == 0) {
            return decenas[decena - 1];
        }
        else if (unidad == 1) {
            return veinte = "VEINTI" + "UNO ";
        }
        else {
            return veinte = "VEINTI" + unidades(unidad);
        }
    } else {

        if (unidad == 0) {
            return decenas[decena - 1] + " ";
        }
        if (unidad == 1) {
            return decenas[decena - 1] + " Y " + "UNO ";
        }

        return decenas[decena - 1] + " Y " + unidades(unidad);
    }
}

function centenas(centena, decena, unidad) {
    var centenas = Array("CIENTO ", "DOSCIENTOS ", "TRESCIENTOS ", "CUATROCIENTOS ", "QUINIENTOS ", "SEISCIENTOS ", "SETECIENTOS ", "OCHOCIENTOS ", "NOVECIENTOS ");

    if (centena == 0 && decena == 0 && unidad == 0) {
        return "";
    }
    if (centena == 1 && decena == 0 && unidad == 0) {
        return "CIEN ";
    }

    if (centena == 0 && decena == 0 && unidad > 0) {
        return unidades(unidad);
    }

    if (decena == 0 && unidad == 0) {
        return centenas[centena - 1] + "";
    }

    if (decena == 0) {
        var numero = centenas[centena - 1] + "" + decenas(decena, unidad);
        return numero.replace(" Y ", " ");
    }
    if (centena == 0) {

        return decenas(decena, unidad);
    }

    return centenas[centena - 1] + "" + decenas(decena, unidad);

}

function unidadesdemillar(unimill, centena, decena, unidad) {
    var numero = unidades(unimill) + "MIL " + centenas(centena, decena, unidad);
    numero = numero.replace("UN MIL ", "MIL ");
    if (unidad == 0) {
        return numero.replace(" Y ", " ");
    } else {
        return numero;
    }
}

function decenasdemillar(decemill, unimill, centena, decena, unidad) {
    var numero = decenas(decemill, unimill) + "MIL " + centenas(centena, decena, unidad);
    return numero;
}

function centenasdemillar(centenamill, decemill, unimill, centena, decena, unidad) {

    var numero = 0;
    numero = centenas(centenamill, decemill, unimill) + "MIL " + centenas(centena, decena, unidad);

    return numero;
}

function unidadesdemillon(unimillon, centenamill, decemill, unimill, centena, decena, unidad) {
    var numero = unidades(unimillon) + "MILLONES " + centenas(centenamill, decemill, unimill) + "MIL " + centenas(centena, decena, unidad);
    numero = numero.replace("UN MILLONES ", "UN MILLÓN ");
    if (unidad == 0) {
        return numero.replace(" Y ", " ");
    } else {
        return numero;
    }
}

function decenasdemillon(decemillon, unimillon, centenamill, decemill, unimill, centena, decena, unidad) {
    var numero = decenas(decemillon, unimillon) + "MILLONES " + centenas(centenamill, decemill, unimill) + "MIL " + centenas(centena, decena, unidad);
    return numero;
}

function centenasdemillon(centenamillon, decemillon, unimillon, centenamill, decemill, unimill, centena, decena, unidad) {

    var numero = 0;
    numero = centenas(centenamillon, decemillon, unimillon) + "MILLONES " + centenas(centenamill, decemill, unimill) + "MIL " + centenas(centena, decena, unidad);

    return numero;
}

function separar_split(texto) {
    var contenido = new Array();
    for (var i = 0; i < texto.length; i++) {
        contenido[i] = texto.substr(i, 1);
    }
    return contenido;
}