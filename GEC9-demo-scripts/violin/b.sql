SOURCE a.sql;

UPDATE status
SET    status='normal'
WHERE  handle like 'Utah-%-Utah-switch';

UPDATE status
SET    status='alert'
WHERE  handle in ('Utah-A', 'Utah-B', 'Utah-C',
       	      	  'Utah-D', 'Utah-E', 'Utah-F',
		  'Utah-controller', 'Utah-switch',
		  'Utah');

