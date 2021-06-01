CREATE DEFINER=`root`@`localhost` PROCEDURE `GenerateRowNoInfo`(IN InColumnId INT)
BEGIN
	DECLARE loop0_eof INT DEFAULT FALSE;
	DECLARE loop0_column_id INT; 
	DECLARE loop0_table_id INT; 
	DECLARE row_count INT; 
	DECLARE loop0_column_name VARCHAR(255); 
	DECLARE loop0_table_name VARCHAR(255); 
	DECLARE loop0_database_name VARCHAR(255); 
	DECLARE loop0_data_type INT; 
	DECLARE loop0_max_length INT; 
	DECLARE coll_id INT;
	DECLARE cur0 CURSOR FOR 
		SELECT c.`id`, t.`id`, LOWER(c.`name`), t.`name`, t.`database`, c.`data_type`, c.`max_length`
			FROM
				`viewbox`.`columns` c
			JOIN `viewbox`.`tables` t ON
				t.`id`=c.`table_id`
			WHERE  c.`id`=InColumnId;


	DECLARE CONTINUE HANDLER FOR NOT FOUND SET loop0_eof = TRUE;

	OPEN cur0;
		loop0: LOOP
			FETCH cur0 INTO loop0_column_id, loop0_table_id, loop0_column_name, loop0_table_name,loop0_database_name,loop0_data_type,loop0_max_length;
			IF loop0_eof THEN
				leave loop0;
			END IF;



						SET @sss11=CONCAT("
										DROP TABLE IF EXISTS `row_no_",loop0_column_id,"`;
										");
						PREPARE stmt11 FROM @sss11;
						EXECUTE stmt11;


						SET @sss1=CONCAT("
										CREATE TABLE `row_no_",loop0_column_id,"` (
										  `id` INT NOT NULL AUTO_INCREMENT,
										  `from_row` INT NOT NULL,
										  `to_row` INT,
										  `value_id` INT NOT NULL,
										  PRIMARY KEY (`id`)
										) ENGINE=MyISAM;
										");
						PREPARE stmt2 FROM @sss1;
						EXECUTE stmt2;

						BLOCK1: BEGIN
							DECLARE old_value_id INT;
							DECLARE start_rowno INT;
							DECLARE old_rowno INT;

							SET old_value_id = -1;
							SET old_rowno = -1;
							SET start_rowno = 1;
              set @rowno = 0;
              set @rowno = 0;
							WHILE not @rowno is null DO
								set @rowno = null;
								SET @sss2=CONCAT("
										SELECT  v.id, t._row_no_
                    into @value_id, @rowno
										FROM `",loop0_database_name,"`.`",loop0_table_name,"` t
											JOIN
												`value_",loop0_column_id,"` v
											ON
												t.`",loop0_column_name,"`=v.`value`
											WHERE T._ROW_NO_ > ",old_rowno,"
											ORDER BY t._row_no_
											LIMIT 1
										");
								PREPARE stmt2 FROM @sss2;
								EXECUTE stmt2;
                DEALLOCATE PREPARE stmt2;

                if (not @rowno is null) then
                  if (old_value_id <> @value_id and old_value_id <> -1) THEN
                     SET @sss3 = concat("INSERT INTO `row_no_",loop0_column_id,"`(from_row, to_row, value_id) values(
                           ", start_rowno, ", ", old_rowno, ", ", old_value_id, "
                           )");
                     PREPARE stmt3 FROM @sss3;
		   	  					EXECUTE stmt3;
                     SET start_rowno = @rowno;

                  end if;
							  	SET old_rowno = @rowno;
								  SET old_value_id = @value_id;
                end if ;
							END WHILE;

              if (old_value_id <> -1) THEN
                   SET @sss3 = concat("INSERT INTO `row_no_",loop0_column_id,"`(from_row, to_row, value_id) values(
                         ", start_rowno, ", ", old_rowno, ", ", old_value_id, "
                         )");
                   PREPARE stmt3 FROM @sss3;
		   						EXECUTE stmt3;
                end if;
						END BLOCK1;
	END LOOP;
	CLOSE cur0;

END