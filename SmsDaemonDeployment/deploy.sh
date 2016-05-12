#!/bin/sh

cp onReceive.sh /var/spool/gammu/
chown gammu:gammu /var/spool/gammu/onReceive.sh
chmod +x /var/spool/gammu/onReceive.sh

cp gammu-smsdrc /etc/

mkdir /var/spool/doorbell/
chown gammu:gammu /var/spool/doorbell/

/etc/init.d/gammu-smsd restart
