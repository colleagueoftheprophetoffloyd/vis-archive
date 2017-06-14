#!/bin/bash
mysql -h bain.gpolab.bbn.com -u pathlet --password='p@thlet' << EOF
use pathlet;
update status set status='forward' where handle='pathlet1';
update status set status='normal' where handle='pathlet2';
EOF

