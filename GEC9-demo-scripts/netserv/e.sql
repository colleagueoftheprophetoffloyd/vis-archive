UPDATE	status
SET	status = 'normal';

UPDATE	status
SET	status = 'forward'
WHERE	handle in ('Router1-Client1B');

UPDATE	status
SET	status = 'backward'
WHERE	handle in ('Router1-Client1A');

UPDATE	status
SET	status = 'rainbow'
WHERE	handle in ('Router1');
