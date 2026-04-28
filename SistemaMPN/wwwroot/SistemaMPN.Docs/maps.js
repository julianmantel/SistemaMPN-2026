let map;
let ubicacionPrincipal = [-31.741130084218355, -60.50015151397077];
let rutaActual = null;
let marcadorPrincipal = null;
let marcadorActual = null;
let isAdminOrGestor;

window.setDotNetRef = function (ref) {
    window._dotRef = ref;
};

function iniciarMapa(isAuthorized = false) {
    map = L.map('map').setView(ubicacionPrincipal, 28);

    isAdminOrGestor = isAuthorized;

    let marcador = L.icon({
        iconUrl: 'Icons/iconIglesia.png',
        iconSize: [42, 50],
        iconAnchor: [21, 50],
        popupAnchor: [0, -50]
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    marcadorPrincipal = L.marker(ubicacionPrincipal, { icon: marcador }).addTo(map);
    map.on("dblclick", onDbClickMap);
    marcadorPrincipal.on("click", onClickMark);
}

function cargarLocalizacion(tipo, lat, lng) {

    let iconMark;
    let ubicacion = [lat, lng];

    if (tipo == "Celula") {
        iconMark = L.icon({
            iconUrl: 'Icons/iconCelula.png',
            iconSize: [42, 50],
            iconAnchor: [21, 50],
            popupAnchor: [0, -50]
        });
    } else {
        iconMark = L.icon({
            iconUrl: 'Icons/iconCasaDeOracion.png',
            iconSize: [42, 50],
            iconAnchor: [21, 50],
            popupAnchor: [0, -50]
        });
    }

    let marcadorAux = L.marker(ubicacion, { icon: iconMark }).addTo(map);
    marcadorPrincipal = marcadorAux;

    marcadorAux.on("click", onClickMark);

}

function removeMark(removeRoute = true) {
    if (marcadorPrincipal) {
        if (rutaActual && removeRoute) {
            map.removeControl(rutaActual);
        }
        map.removeLayer(marcadorPrincipal);
        ubicacionPrincipal = [-31.741130084218355, -60.50015151397077];
    }
}

function onClickMark() {
    marcadorPrincipal = this;

    let ubicacion = this.getLatLng();

    if (ubicacionPrincipal[0] == ubicacion.lat && ubicacionPrincipal[1] == ubicacion.lng) return;

    ubicacionPrincipal = [ubicacion.lat, ubicacion.lng];

    if (marcadorActual) {
        let pos = marcadorActual.getLatLng();
        formarRuta([pos.lat, pos.lng], ubicacionPrincipal);
    }
    ocultarContenedorRouting();
}

function onDbClickMap(e) {
    if (marcadorActual) {
        map.removeLayer(marcadorActual);
    }

    let marcadorDireccion = [e.latlng.lat, e.latlng.lng];

    marcadorActual = L.marker(marcadorDireccion, { draggable: true })
        .addTo(map)
        .on('dragend', onMoveMarcador);

    formarRuta(marcadorDireccion, ubicacionPrincipal);
    ocultarContenedorRouting();

    if (isAdminOrGestor) {
        marcadorActual.bindPopup('<button onclick="openDialog()" class="mud-button-root mud-button mud-button-filled mud-button-filled-primary">Agregar localizacion</button>').openPopup();
    }

    document.getElementById('map').style.cursor = "";
}

function formarRuta(direccionDesde, direccionHasta) {
    if (rutaActual) {
        map.removeControl(rutaActual);
    }

    rutaActual = L.Routing.control({
        waypoints: [
            L.latLng(direccionDesde[0], direccionDesde[1]),
            L.latLng(direccionHasta[0], direccionHasta[1])
        ],
        routeWhileDragging: false,
        language: 'es',
        collapsible: false,
        show: false,
        addWaypoints: false,
        collapseBtnClass: '',
        draggableWaypoints: false,
        suppressDemoServerWarning: true,
        createMarker: () => null
    }).addTo(map);
}

function onMoveMarcador(e) {
    marcadorActual = e.target;
    let posicion = marcadorActual.getLatLng();

    formarRuta([posicion.lat, posicion.lng], ubicacionPrincipal);
    ocultarContenedorRouting();
}

function mostrarUbicacion(ubicacionActual = false, lat = -31.741130084218355, lng = -60.50015151397077) {
    let ubicacion;

    if (navigator.geolocation && ubicacionActual) {
        navigator.geolocation.getCurrentPosition((position) => {
            if (marcadorActual) {
                map.removeLayer(marcadorActual);
            }

            ubicacion = [position.coords.latitude, position.coords.longitude];
            marcadorActual = L.marker(ubicacion, { draggable: true })
                .addTo(map)
                .on('dragend', onMoveMarcador);

            formarRuta(ubicacion, ubicacionPrincipal);

            if (isAdminOrGestor) {
                marcadorActual.bindPopup('<button onclick="openDialog()" class="mud-button-root mud-button mud-button-filled mud-button-filled-primary">Agregar localizacion</button>').openPopup();
            }

            map.panTo(ubicacion, 28);
            ocultarContenedorRouting();
        });
    } else {

        ubicacionPrincipal = [lat, lng];

        if (marcadorPrincipal) {
            map.eachLayer((marcador) => {
                if (marcador instanceof L.Marker) {
                    if (marcador.getLatLng().equals(ubicacionPrincipal)) {
                        marcadorPrincipal = marcador;
                    }
                }
            });
        }

        if (marcadorActual != null) {
            let posicion = marcadorActual.getLatLng();

            formarRuta([posicion.lat, posicion.lng], ubicacionPrincipal);
        }

        ocultarContenedorRouting();
    }
}

function openDialog() {
    if (window._dotRef && marcadorActual) {
        let coords = marcadorActual.getLatLng();
        window._dotRef.invokeMethodAsync("CrearLocalizacion", coords.lat, coords.lng);

        marcadorActual.closePopup();
    }
}

function ocultarContenedorRouting() {
    const routingContainers = document.getElementsByClassName('leaflet-routing-container');
    if (routingContainers.length > 0) {
        routingContainers[0].style.display = 'none';
    }
    document.querySelectorAll('.leaflet-routing-container').forEach(c => c.style.color = 'black');
}