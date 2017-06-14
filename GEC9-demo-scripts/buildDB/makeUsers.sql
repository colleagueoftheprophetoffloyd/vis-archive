USE mysql;

-- Privileged users

CREATE USER mberman IDENTIFIED BY PASSWORD '3aced2a67029e994';
CREATE USER jkarlin IDENTIFIED BY PASSWORD '646552f6355f2ee4';
CREATE USER nriga IDENTIFIED BY PASSWORD '01010a1b51e95581';
CREATE USER manu IDENTIFIED BY PASSWORD '76822fc7234a5fae';

GRANT ALL PRIVILEGES on *.* TO mberman@'%' WITH GRANT OPTION;
GRANT ALL PRIVILEGES on *.* TO jkarlin@'%' WITH GRANT OPTION;
GRANT ALL PRIVILEGES on *.* TO nriga@'%' WITH GRANT OPTION;
GRANT ALL PRIVILEGES on *.* TO manu@'%' WITH GRANT OPTION;


-- Demo users

CREATE USER smartre IDENTIFIED BY 'sm@rtre';
GRANT ALL PRIVILEGES on smartre.* to smartre@'%';
GRANT SELECT on GEC9_demos.* to smartre@'%';

CREATE USER pathlet IDENTIFIED BY 'p@thlet';
GRANT ALL PRIVILEGES on pathlet.* to pathlet@'%';
GRANT SELECT on GEC9_demos.* to pathlet@'%';

CREATE USER netserv IDENTIFIED BY '^etserv';
GRANT ALL PRIVILEGES on netserv.* to netserv@'%';
GRANT SELECT on GEC9_demos.* to netserv@'%';

CREATE USER violin IDENTIFIED BY 'vi0lin';
GRANT ALL PRIVILEGES on violin.* to violin@'%';
GRANT SELECT on GEC9_demos.* to violin@'%';

CREATE USER cdn IDENTIFIED BY 'cd^';
GRANT ALL PRIVILEGES on cdn.* to cdn@'%';
GRANT ALL PRIVILEGES on cdndetail.* to cdn@'%';
GRANT ALL PRIVILEGES on cdnemily.* to cdn@'%';
GRANT ALL PRIVILEGES on cdnkayleea.* to cdn@'%';
GRANT ALL PRIVILEGES on cdnkayleeb.* to cdn@'%';
GRANT SELECT on GEC9_demos.* to cdn@'%';
GRANT SELECT on jkarlin.* to cdn@'%';

CREATE USER nowcast IDENTIFIED BY 'nowc@st';
GRANT ALL PRIVILEGES on nowcast.* to nowcast@'%';
GRANT SELECT on GEC9_demos.* to nowcast@'%';

CREATE USER parknet IDENTIFIED BY 'p@rknet';
GRANT ALL PRIVILEGES on parknet.* to parknet@'%';
GRANT SELECT on GEC9_demos.* to parknet@'%';

FLUSH PRIVILEGES;
