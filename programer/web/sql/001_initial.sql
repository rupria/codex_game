CREATE TABLE IF NOT EXISTS guest_saves (
  guest_id CHAR(36) NOT NULL,
  save_version INT UNSIGNED NOT NULL,
  save_data JSON NOT NULL,
  created_at TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
  updated_at TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3)
    ON UPDATE CURRENT_TIMESTAMP(3),
  PRIMARY KEY (guest_id)
);

CREATE TABLE IF NOT EXISTS match_results (
  match_id CHAR(36) NOT NULL,
  guest_id CHAR(36) NOT NULL,
  result VARCHAR(24) NOT NULL,
  stage INT UNSIGNED NOT NULL,
  duration_ms INT UNSIGNED NOT NULL,
  input_time_ms INT UNSIGNED NULL,
  content_version VARCHAR(64) NOT NULL,
  played_at TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP(3),
  PRIMARY KEY (match_id),
  KEY idx_match_results_guest_played (guest_id, played_at),
  KEY idx_match_results_version_played (content_version, played_at)
);
