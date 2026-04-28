BEGIN;
CREATE TABLE direcciones (
	   id_direccion SERIAL PRIMARY KEY, 
	   calle VARCHAR(250),
	   altura INT,
	   barrio VARCHAR(250)
);
CREATE TABLE datos_personales (
	   id_datos_personales SERIAL PRIMARY KEY,
	   telefono_alternativo VARCHAR(100),
	   estado_civil VARCHAR(15) NOT NULL,
	   pareja VARCHAR(250)
);
CREATE TABLE hijos (
	   id_hijos SERIAL PRIMARY KEY,
	   nombre VARCHAR(100) NOT NULL,
	   apellido VARCHAR(100) NOT NULL
);
CREATE TABLE datos_personales_hijos (
	   id_hijos INT,
	   id_datos_personales INT,
	   PRIMARY KEY (id_hijos, id_datos_personales),
	   
	   FOREIGN KEY (id_hijos) REFERENCES hijos(id_hijos) ON DELETE CASCADE,
	   FOREIGN KEY (id_datos_personales) REFERENCES datos_personales(id_datos_personales) ON DELETE CASCADE
);
CREATE TABLE trayectorias (
	   id_trayectoria SERIAL PRIMARY KEY,
	   estudios_primario VARCHAR(20) NOT NULL,
	   estudios_secundario VARCHAR(20) NOT NULL,
	   estudios_terciario VARCHAR(20) NOT NULL,
	   estudios_universitario VARCHAR(20) NOT NULL,
	   carrera VARCHAR(200),
	   situacion_laboral VARCHAR(60),
	   rubro VARCHAR(100),
	   cursos_realizados VARCHAR(255)
);
CREATE TABLE condiciones_medicas (
	   id_condicion_medica SERIAL PRIMARY KEY,
	   condicion VARCHAR(100)
);
CREATE TABLE medicamentos (
	   id_medicamento SERIAL PRIMARY KEY,
	   nombre VARCHAR(100) NOT NULL
);
CREATE TABLE alergias (
	   id_alergia SERIAL PRIMARY KEY,
	   nombre VARCHAR(100)
);
CREATE TABLE informaciones_salud (
	   id_informacion_salud SERIAL PRIMARY KEY,
	   grupo_sanguineo VARCHAR(10),
	   observaciones VARCHAR(255)
);
CREATE TABLE telefonos_emergencias (
	   id_telefonos_emergencia SERIAL PRIMARY KEY,
	   id_informacion_salud INT,
	   telefono VARCHAR(20),
	   propietario VARCHAR(250),

	   FOREIGN KEY (id_informacion_salud) REFERENCES informaciones_salud(id_informacion_salud) ON DELETE CASCADE
);
CREATE TABLE salud_condiciones (
	   id_informacion_salud INT,
	   id_condicion_medica INT,
	   PRIMARY KEY(id_informacion_salud, id_condicion_medica),

	   FOREIGN KEY (id_informacion_salud) REFERENCES informaciones_salud(id_informacion_salud) ON DELETE CASCADE,
	   FOREIGN KEY (id_condicion_medica) REFERENCES condiciones_medicas(id_condicion_medica) ON DELETE CASCADE
);
CREATE TABLE salud_medicamentos (
	   id_informacion_salud INT,
	   id_medicamento INT,
	   PRIMARY KEY(id_informacion_salud, id_medicamento),

	   FOREIGN KEY (id_informacion_salud) REFERENCES informaciones_salud(id_informacion_salud) ON DELETE CASCADE,
	   FOREIGN KEY (id_medicamento) REFERENCES medicamentos(id_medicamento) ON DELETE CASCADE
);
CREATE TABLE salud_alergias (
	   id_informacion_salud INT,
	   id_alergia INT,
	   PRIMARY KEY(id_informacion_salud, id_alergia),

	   FOREIGN KEY (id_informacion_salud) REFERENCES informaciones_salud(id_informacion_salud) ON DELETE CASCADE,
	   FOREIGN KEY (id_alergia) REFERENCES alergias(id_alergia) ON DELETE CASCADE
);
CREATE TABLE bautismos (
	   id_bautismos SERIAL PRIMARY KEY,
	   fecha INT,
	   lugar VARCHAR(60),
	   pastor VARCHAR(100),
	   realizo BOOLEAN
);
CREATE TABLE informaciones_eclesiasticas (
	   id_informacion_eclesiastica SERIAL PRIMARY KEY,
	   id_bautismos INT,
	   convocante VARCHAR(75),
	   fecha_asiste INT,

	   FOREIGN KEY (id_bautismos) REFERENCES bautismos(id_bautismos)
);
CREATE TABLE seminarios (
	   id_seminarios SERIAL PRIMARY KEY,
	   nombre VARCHAR(150),
	   anio_comienzo INT,
	   activo BOOLEAN
);
CREATE TABLE seminarios_cursados (
	   id_seminarios INT,
	   id_informacion_eclesiastica INT,
	   anio_cursado INT,
	   estado VARCHAR(20),
	   
	   PRIMARY KEY(id_seminarios, id_informacion_eclesiastica),

	   FOREIGN KEY (id_seminarios) REFERENCES seminarios(id_seminarios) ON DELETE CASCADE,
	   FOREIGN KEY (id_informacion_eclesiastica) REFERENCES informaciones_eclesiasticas(id_informacion_eclesiastica) ON DELETE CASCADE
);
CREATE TABLE miembros (
	   id_miembros SERIAL PRIMARY KEY,
	   dni VARCHAR(15) NOT NULL UNIQUE,
	   nombre VARCHAR(100) NOT NULL,
	   apellido VARCHAR(100) NOT NULL,
	   fecha_nacimiento DATE NOT NULL,
	   fecha_hasta DATE,
	   nacionalidad VARCHAR(150) NOT NULL,
	   lugar_nacimiento VARCHAR(100),
	   telefono VARCHAR(20),
	   telefono_fijo VARCHAR(20),
	   sexo CHAR(1) NOT NULL DEFAULT 'M',
	   fecha_creacion TIMESTAMPTZ,
	   id_direccion INT UNIQUE,
	   id_datos_personales INT UNIQUE,
	   id_trayectoria INT UNIQUE,
	   id_informacion_salud INT UNIQUE,
	   id_informacion_eclesiastica INT UNIQUE,

	   FOREIGN KEY (id_datos_personales) REFERENCES datos_personales(id_datos_personales),
	   FOREIGN KEY (id_trayectoria) REFERENCES trayectorias(id_trayectoria),
	   FOREIGN KEY (id_informacion_salud) REFERENCES informaciones_salud(id_informacion_salud),
	   FOREIGN KEY (id_informacion_eclesiastica) REFERENCES informaciones_eclesiasticas(id_informacion_eclesiastica)
);
CREATE TABLE roles (
	   id_rol SERIAL PRIMARY KEY,
	   nombre VARCHAR(80) NOT NULL DEFAULT 'Usuario'
);
CREATE TABLE usuarios (
	   id_usuarios SERIAL PRIMARY KEY,
	   user_name VARCHAR(100),
	   correo VARCHAR(150),
	   password_hash BYTEA,
       password_salt BYTEA,
	   id_miembros INT UNIQUE,
	   
	   FOREIGN KEY (id_miembros) REFERENCES miembros(id_miembros) ON DELETE CASCADE
);
CREATE TABLE roles_usuarios (
	  id_rol INT,
	  id_usuarios INT,
	  PRIMARY KEY(id_rol, id_usuarios),

	  FOREIGN KEY (id_rol) REFERENCES roles(id_rol) ON DELETE CASCADE,
	  FOREIGN KEY (id_usuarios) REFERENCES usuarios(id_usuarios) ON DELETE CASCADE
);
CREATE TABLE lideres (
	   id_miembros INT PRIMARY KEY,
	   tipo VARCHAR(30),

	   FOREIGN KEY (id_miembros) REFERENCES miembros(id_miembros) ON DELETE CASCADE
);
CREATE TABLE gestores_miembros (
	   id_miembros INT PRIMARY KEY,

	   FOREIGN KEY (id_miembros) REFERENCES miembros(id_miembros) ON DELETE CASCADE
);
CREATE TABLE consultores (
	   id_miembros INT PRIMARY KEY,

	   FOREIGN KEY (id_miembros) REFERENCES miembros(id_miembros) ON DELETE CASCADE
);
CREATE TABLE tesoreros (
	   id_miembros INT PRIMARY KEY,
	   is_pro BOOLEAN DEFAULT false,
	   
	   FOREIGN KEY (id_miembros) REFERENCES miembros(id_miembros) ON DELETE CASCADE
);
CREATE TABLE localizaciones (
	   id_localizaciones SERIAL PRIMARY KEY,
	   tipo TEXT,
	   direccion TEXT,
	   ubicacion POINT NOT NULL
);
CREATE TABLE grupos (
	   id_grupos SERIAL PRIMARY KEY,
	   nombre VARCHAR(200),
	   cantidad_miembros INT,
	   max_cant_miembros INT DEFAULT 12,
	   id_localizaciones INT,

	   FOREIGN KEY (id_localizaciones) REFERENCES localizaciones(id_localizaciones)
);
CREATE TABLE pertenece_grupo (
	   id_grupos INT,
	   id_miembros INT,
	   PRIMARY KEY(id_grupos, id_miembros),
	   fecha_desde DATE ,
	   ocupacion VARCHAR(60),

	   FOREIGN KEY (id_grupos) REFERENCES grupos(id_grupos) ON DELETE CASCADE,
	   FOREIGN KEY (id_miembros) REFERENCES miembros(id_miembros) ON DELETE CASCADE
);
CREATE TABLE peticiones (
	   id_peticiones SERIAL PRIMARY KEY,
	   fecha_solicitud TIMESTAMP NOT NULL,
	   fecha_respuesta TIMESTAMP,
	   estado VARCHAR(30) NOT NULL DEFAULT 'En proceso',
	   mensaje TEXT,
	   tipo VARCHAR(40),
	   id_usuario INT,
	   id_lider INT,

	   CONSTRAINT fk_peticiones_usuario FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuarios) ON DELETE SET NULL,
	   FOREIGN KEY (id_lider) REFERENCES miembros(id_miembros) ON DELETE CASCADE
);

CREATE TABLE peticiones_agregar (
	   id_peticiones SERIAL PRIMARY KEY,
	   dni VARCHAR(15),
	   nombre VARCHAR(100) NOT NULL,
	   apellido VARCHAR(100) NOT NULL,
	   fecha_nacimiento DATE,
	   nacionalidad VARCHAR(150),
	   lugar_nacimiento VARCHAR(100),
	   telefono VARCHAR(20),
	   sexo CHAR(1) DEFAULT 'M',

	   FOREIGN KEY (id_peticiones) REFERENCES peticiones(id_peticiones) ON DELETE CASCADE
);

CREATE TABLE peticiones_cambio (
	id_peticiones INT PRIMARY KEY,
	id_grupos INT,
	id_miembros INT,
	
	FOREIGN KEY (id_peticiones) REFERENCES peticiones(id_peticiones) ON DELETE CASCADE,
	FOREIGN KEY (id_grupos) REFERENCES grupos(id_grupos) ON DELETE CASCADE,
	FOREIGN KEY (id_miembros) REFERENCES miembros(id_miembros) ON DELETE CASCADE
);

CREATE TABLE peticiones_actualizacion (
	id_peticiones INT PRIMARY KEY,
	id_miembros INT,

	FOREIGN KEY (id_peticiones) REFERENCES peticiones(id_peticiones) ON DELETE CASCADE,
	FOREIGN KEY (id_miembros) REFERENCES miembros(id_miembros) ON DELETE CASCADE
);

CREATE TABLE peticiones_reunion (
	id_peticiones INT PRIMARY KEY,
	motivo TEXT,
	fecha_preferida DATE,
	correo TEXT,
	id_miembros INT,
	id_consultor INT,

	FOREIGN KEY (id_peticiones) REFERENCES peticiones(id_peticiones) ON DELETE CASCADE,
	FOREIGN KEY (id_miembros) REFERENCES miembros(id_miembros) ON DELETE CASCADE,
	FOREIGN KEY (id_consultor) REFERENCES consultores(id_miembros) ON DELETE CASCADE
);

CREATE TABLE peticion_cambiar_password
(
    id_peticion_cambiar_password SERIAL PRIMARY KEY,
    reset_token VARCHAR(255),
    reset_token_expire timestamp with time zone,
    id_usuarios INT UNIQUE,
    FOREIGN KEY (id_usuarios) REFERENCES usuarios (id_usuarios) ON DELETE CASCADE
);
CREATE TABLE eventos
(
	id_evento SERIAL PRIMARY KEY,
	titulo TEXT,
	lugar TEXT,
	calle TEXT,
	altura INT,
	color VARCHAR(10),
	fecha TIMESTAMP,
	hora_inicio INTERVAL,
	hora_fin INTERVAL,
	duracion_completa BOOLEAN
);

CREATE TABLE eventos_rol
(
	id_evento INT,
	id_rol INT,
	PRIMARY KEY (id_evento, id_rol),

	FOREIGN KEY (id_evento) REFERENCES eventos (id_evento) ON DELETE CASCADE,
	FOREIGN KEY (id_rol) REFERENCES roles (id_rol) ON DELETE CASCADE
);

CREATE TABLE notificaciones (
	id_notificaciones SERIAL PRIMARY KEY,
	mensaje TEXT,
	tipo VARCHAR(60),
	fecha TIMESTAMP
);

CREATE TABLE notificacion_usuario (
	id_notificacion INT,
	id_usuario INT,
	leida BOOLEAN DEFAULT false,
	PRIMARY KEY (id_notificacion, id_usuario),

	FOREIGN KEY (id_notificacion) REFERENCES notificaciones (id_notificaciones) ON DELETE CASCADE,
	FOREIGN KEY (id_usuario) REFERENCES usuarios (id_usuarios) ON DELETE CASCADE
);

CREATE TABLE turnos (
	id_turnos SERIAL PRIMARY KEY,
	color VARCHAR(10),
	fecha TIMESTAMP,
	hora_inicio INTERVAL,
	id_tesorero INT,
	
	FOREIGN KEY (id_tesorero) REFERENCES tesoreros(id_miembros)
);

CREATE TABLE propuesta_cambio_turno (
	id_propuesta_cambio_turno SERIAL PRIMARY KEY,
	estado VARCHAR(30) NOT NULL DEFAULT 'En proceso',
	fecha_solicitud TIMESTAMP NOT NULL,
	fecha_respuesta TIMESTAMP,
	id_emisor INT,
	id_receptor INT,
	id_turno INT,
	
	FOREIGN KEY (id_receptor) REFERENCES tesoreros(id_miembros),
	FOREIGN KEY (id_turno) REFERENCES turnos(id_turnos)
);

CREATE TABLE documentos (
	id_documentos SERIAL PRIMARY KEY,
	fecha DATE,
	nro INT,
	tipo VARCHAR(100),
	nombre_archivo_original varchar(255),
	nombre_archivo_firmado varchar(255),
	firmado BOOLEAN DEFAULT FALSE
); 

CREATE TABLE documentos_tesoreros (
	id_documentos INT,
	id_tesorero INT,
	es_creador BOOLEAN,
	es_supervisor BOOLEAN,
	
	FOREIGN KEY (id_documentos) REFERENCES documentos(id_documentos),
	FOREIGN KEY (id_tesorero) REFERENCES tesoreros(id_miembros),
	PRIMARY KEY (id_documentos,id_tesorero)
);

CREATE TABLE reuniones (
	id_reunion SERIAL PRIMARY KEY,
	motivo TEXT,
	fecha TIMESTAMP,
	correo VARCHAR(200),
	estado VARCHAR(100),
	id_miembro INT,
	id_consultor INT,
	id_peticion INT,

	FOREIGN KEY (id_miembro) REFERENCES miembros (id_miembros) ON DELETE CASCADE,
	FOREIGN KEY (id_consultor) REFERENCES consultores (id_miembros) ON DELETE CASCADE
);

CREATE TABLE notas(
	id_nota SERIAL PRIMARY KEY, 
	comentarios TEXT,
	id_reunion INT,
	FOREIGN KEY (id_reunion) REFERENCES reuniones (id_reunion) ON DELETE CASCADE
);

CREATE TABLE asistencias (
	id_asistencia INT GENERATED BY DEFAULT AS IDENTITY,
	id_miembro INT,
	id_evento INT NOT NULL,
	nombre_visitante character varying(100),
	apellido_visitante character varying(100),
	email_visitante character varying(150),
	fecha_registro timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
	codigo_verificacion character varying(50) NOT NULL,
	origen character varying(20) NOT NULL DEFAULT 'QR',
	CONSTRAINT "PK_asistencias" PRIMARY KEY (id_asistencia),
	CONSTRAINT "FK_asistencias_eventos_id_evento" FOREIGN KEY (id_evento) REFERENCES eventos (id_evento) ON DELETE CASCADE,
	CONSTRAINT "FK_asistencias_miembros_id_miembro" FOREIGN KEY (id_miembro) REFERENCES miembros (id_miembros) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS ix_asistencias_codigo_verificacion 
	ON asistencias (codigo_verificacion);
CREATE INDEX IF NOT EXISTS "IX_asistencias_id_evento" 
	ON asistencias (id_evento);
CREATE UNIQUE INDEX IF NOT EXISTS ix_asistencias_miembro_evento 
	ON asistencias (id_miembro, id_evento) WHERE id_miembro IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_asistencias_email_evento 
	ON asistencias (email_visitante, id_evento) WHERE email_visitante IS NOT NULL;
--ROLLBACK
--COMMIT