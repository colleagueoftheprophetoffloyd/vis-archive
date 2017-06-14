UPDATE status
SET    status='normal';

UPDATE status
SET    status='forward'
WHERE  handle like 'GPO-%-GPO-switch';
