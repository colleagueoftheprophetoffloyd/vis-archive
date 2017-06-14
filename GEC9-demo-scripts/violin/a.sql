UPDATE status
SET    status='normal';

UPDATE status
SET    status='forward'
WHERE  handle in ('Utah-GPO')
OR     handle like 'Utah-%-Utah-switch';
