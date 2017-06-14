UPDATE	status
SET	status = 'normal';

UPDATE	status
SET	status = 'throb'
WHERE	handle in ('Client1A', 'Router1', 'KansasCity', 'Client1B');

