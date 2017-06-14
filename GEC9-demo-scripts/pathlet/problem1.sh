#!/bin/bash
mysql -h bain.gpolab.bbn.com -u pathlet --password='p@thlet' << EOF
use pathlet;
update status set status='throb' where handle='pathlet1';
EOF

