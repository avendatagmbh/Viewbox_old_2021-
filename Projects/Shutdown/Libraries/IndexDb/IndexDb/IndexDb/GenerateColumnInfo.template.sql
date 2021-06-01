-- --------------------------------------------------------------------------------
-- Routine DDL
-- Note: comments before and after the routine body will not be stored by the server
-- --------------------------------------------------------------------------------

CREATE PROCEDURE `GenerateColumnInfo`(IN InColumnId INT(11))
BEGIN	
	DECLARE loop0_eof INT DEFAULT FALSE; 
	DECLARE loop0_column_id INT(11); 
	DECLARE loop0_table_id INT(11); 
	DECLARE row_count INT(11); 
	DECLARE loop0_column_name VARCHAR(255); 
	DECLARE loop0_table_name VARCHAR(255); 
	DECLARE loop0_database_name VARCHAR(255); 
	DECLARE loop0_data_type INT(11); 
	DECLARE loop0_max_length INT(11); 
	DECLARE coll_id INT(11);
	DECLARE cur0 CURSOR FOR 
		SELECT c.`id`, t.`id`, LOWER(c.`name`), t.`name`, t.`database`, c.`data_type`, c.`max_length`
			FROM 
				`viewbox_zsv`.`columns` c
			JOIN `viewbox_zsv`.`tables` t ON
				t.`id`=c.`table_id`
			WHERE  c.`id`=InColumnId;


	DECLARE CONTINUE HANDLER FOR NOT FOUND SET loop0_eof = TRUE;

	
	OPEN cur0; 
		loop0: LOOP 
			FETCH cur0 INTO loop0_column_id, loop0_table_id, loop0_column_name, loop0_table_name,loop0_database_name,loop0_data_type,loop0_max_length;
			IF loop0_eof THEN
				leave loop0;
			END IF;


						SET @type = "";
						-- loop0_max_length
						SET @string_length = loop0_max_length;
						
						if (loop0_data_type = 0) then
							CASE @string_length
							WHEN 0 THEN SET @string_length = 128;
							ELSE SET @string_length = loop0_max_length;
						END CASE;
						end if;
												
						CASE loop0_data_type
							WHEN 0 THEN SET @type = CONCAT("VARCHAR(", @string_length, ")");	-- ok
							WHEN 1 THEN SET @type = "INT(11)";		-- ok
							WHEN 2 THEN SET @type = CONCAT("DECIMAL(25, ", @string_length, ")"); -- max 65, max 30
							WHEN 3 THEN SET @type = CONCAT("DECIMAL(25, ", @string_length, ")"); -- max 65, max 30
							WHEN 4 THEN SET @type = "DATE";			-- ok
							WHEN 5 THEN SET @type = "TINYINT(1)";	-- ok
							WHEN 8 THEN SET @type = "TIME";			-- ok
							WHEN 12 THEN SET @type = "DATETIME";		-- ok
							ELSE SET @type = @type = "'VARCHAR(128)";	-- ok
						END CASE;
						
						
						SET @sss11=CONCAT("
										DROP TABLE IF EXISTS `value_",loop0_column_id,"`;
										");
						PREPARE stmt11 FROM @sss11;
						EXECUTE stmt11;
						
						SET @sss22=CONCAT("
										DROP TABLE IF EXISTS `index_",loop0_column_id,"`;
										");
						PREPARE stmt22 FROM @sss22;
						EXECUTE stmt22;

						SET @sss22=CONCAT("
										DROP TABLE IF EXISTS `temp_order_areas_",loop0_column_id,"`;
										");
						PREPARE stmt22 FROM @sss22;
						EXECUTE stmt22;
						
						SET @sss1=CONCAT("										
										CREATE TABLE IF NOT EXISTS `value_",loop0_column_id,"` ( 
										  `id` int(11),
										  `value` ", @type, " DEFAULT NULL,
                                          INDEX `value_idx` (`value`)
										) ENGINE=MyISAM;
										");

						PREPARE stmt1 FROM @sss1;
						EXECUTE stmt1;


						SET @sss1=CONCAT("										
										CREATE TABLE IF NOT EXISTS `temp_order_areas_",loop0_column_id,"` ( 
										  `id` int(11) NOT NULL,
										  `start` bigint(11),
										  `end` bigint(11),
										  PRIMARY KEY (`id`),
                                          INDEX `all` (`id` ASC, `start` ASC, `end` ASC),
										  INDEX `all2` (`start` ASC, `end` ASC, `id` ASC)
										) ENGINE=MyISAM;
										");

						PREPARE stmt1 FROM @sss1;
						EXECUTE stmt1;

						SET @sss1=CONCAT("										
										insert into `temp_order_areas_",loop0_column_id,"` ( `id`, `start`, `end`)
										select `id`, `start`, `end` from viewbox_zsv.order_areas 
										where table_id=",loop0_table_id," ;
										");

						PREPARE stmt1 FROM @sss1;
						EXECUTE stmt1;

						SET @sss2=CONCAT("										
										CREATE TABLE IF NOT EXISTS `index_",loop0_column_id,"` (
										  `value_id` int(11),
										  `order_areas_id` int(11),
                                          INDEX `value_id_idx` (`value_id`),
                                          INDEX `order_areas_id_idx` (`order_areas_id`)
										) ENGINE=MyISAM;
										");
                                        
						PREPARE stmt2 FROM @sss2;
						EXECUTE stmt2;
						
						BLOCK_DONE: BEGIN
							DECLARE failed INT DEFAULT FALSE;
							DECLARE CONTINUE HANDLER FOR SQLEXCEPTION SET failed = TRUE;
							  BEGIN 
                              
								SET @sss3=CONCAT("
									INSERT INTO `value_",loop0_column_id,"` 
									(`id`,`value`)
									SELECT _row_no_, `",loop0_column_name,"`
									FROM `",loop0_database_name,"`.`",loop0_table_name,"`
									WHERE COALESCE(`",loop0_column_name,"`, '') <> ''
                                    GROUP BY `",loop0_column_name,"`
									");
								PREPARE stmt3 FROM @sss3;
								EXECUTE stmt3;
							  END;
							IF failed THEN							
								SET @sss11=CONCAT("
												DROP TABLE IF EXISTS `value_",loop0_column_id,"`;
												");
								PREPARE stmt11 FROM @sss11;
								EXECUTE stmt11;
						
								SET @sss1=CONCAT("										
												CREATE TABLE IF NOT EXISTS `value_",loop0_column_id,"` ( 
												  `id` int(11) NOT NULL AUTO_INCREMENT,
												  `value` ", @type, " DEFAULT NULL,                                          
												  PRIMARY KEY (`id`),
												  INDEX `value_idx` (`value`)
												) ENGINE=MyISAM;
												");
								PREPARE stmt1 FROM @sss1;
								EXECUTE stmt1;
								
									 SET @sss3=CONCAT("
											INSERT INTO `value_",loop0_column_id,"` 
											(`value`)
											SELECT DISTINCT `",loop0_column_name,"`
											FROM `",loop0_database_name,"`.`",loop0_table_name,"`
											WHERE COALESCE(`",loop0_column_name,"`, '') <> '';
											");
											PREPARE stmt3 FROM @sss3;
											EXECUTE stmt3;
							END IF;
						END BLOCK_DONE;
						
						SET @sss1=CONCAT("
										INSERT INTO INDEX_",loop0_column_id,"(`value_id`, `order_areas_id`)
										SELECT distinct v.`id`, o.`id`
										FROM `VALUE_",loop0_column_id,"` v 
										JOIN `temp_order_areas_",loop0_column_id,"` O ON v.`ID` >= O.`START` AND v.`ID` <= O.`END`
										");	
                        
                        PREPARE stmt22 FROM @sss1;
						EXECUTE stmt22;
						
						SET @sss22=CONCAT("
										DROP TABLE IF EXISTS `temp_order_areas_",loop0_column_id,"`;
										");
						PREPARE stmt22 FROM @sss22;
						EXECUTE stmt22;
						
	END LOOP; 
	CLOSE cur0; 

END