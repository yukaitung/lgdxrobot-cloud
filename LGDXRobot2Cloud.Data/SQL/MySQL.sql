delimiter //

-- Assign Auto Task
DROP PROCEDURE IF EXISTS auto_task_assign_task //
CREATE PROCEDURE auto_task_assign_task (
  IN pRobotId CHAR(36)
)
BEGIN
  DECLARE pTaskId INT;
  DECLARE pFlowId INT;
  DECLARE pProgressId INT;
  DECLARE pProgressOrder INT;
  DECLARE pRunningTasks INT;
  
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    START TRANSACTION;
    ROLLBACK;
    RESIGNAL;
  END;

  -- Ensure no running task for robot
  SELECT COUNT(*) INTO pRunningTasks FROM `Navigation.AutoTasks` AS T
    WHERE T.`AssignedRobotId` = pRobotId
    AND T.`CurrentProgressId` != 1
    AND T.`CurrentProgressId` != 2
    AND T.`CurrentProgressId` != 3
    AND T.`CurrentProgressId` != 4;

  IF pRunningTasks = 0 THEN
    START TRANSACTION;
    SELECT T.`Id`, T.`FlowId` INTO pTaskId, pFlowId FROM `Navigation.AutoTasks` AS T 
      WHERE T.`CurrentProgressId` = 2 AND (T.`AssignedRobotId` = pRobotId OR T.`AssignedRobotId` IS NULL)
      ORDER BY T.`Priority` DESC, T.`AssignedRobotId` DESC, T.`Id`
      LIMIT 1 FOR UPDATE SKIP LOCKED;

    IF pTaskId IS NOT NULL AND pFlowId IS NOT NULL THEN
      SELECT F.`ProgressId`, F.`Order` INTO pProgressId, pProgressOrder FROM `Navigation.FlowDetails` AS F 
        WHERE `FlowId` = pFlowId ORDER BY `Order` LIMIT 1;

      UPDATE `Navigation.AutoTasks`
        SET  `AssignedRobotId`      = pRobotId
            ,`CurrentProgressId`    = pProgressId
            ,`CurrentProgressOrder` = pProgressOrder
            ,`NextToken`            = (SELECT MD5(CONCAT(pRobotId, " ", pTaskId, " ", pProgressId, " ", UTC_TIMESTAMP(6))))
            ,`UpdatedAt`            = UTC_TIMESTAMP(6)
        WHERE `Id` = pTaskId;
    END IF;
    COMMIT;
  END IF;

  IF pTaskId IS NOT NULL THEN
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = pTaskId;
  ELSE
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = NULL;
  END IF;
END //

-- Auto Task Next
DROP PROCEDURE IF EXISTS auto_task_next //
CREATE PROCEDURE auto_task_next (
   IN pRobotId CHAR(36)
  ,IN pTaskId INT
  ,IN pNextToken CHAR(32)
)
BEGIN
  DECLARE pFlowId INT DEFAULT NULL;
  DECLARE pCurrentProgressOrder INT DEFAULT NULL;
  DECLARE pNextProgressId INT DEFAULT NULL;
  DECLARE pNextProgressOrder INT DEFAULT NULL;
  DECLARE pTaskUpdated TINYINT DEFAULT 0;
  
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    START TRANSACTION;
    ROLLBACK;
    RESIGNAL;
  END;

  START TRANSACTION;
  SELECT T.`FlowId`, T.`CurrentProgressOrder` INTO pFlowId, pCurrentProgressOrder 
    FROM `Navigation.AutoTasks` AS T
    WHERE T.`Id` = pTaskId AND T.`AssignedRobotId` = pRobotId AND T.`NextToken` = pNextToken
    LIMIT 1 FOR UPDATE NOWAIT;

  IF pFlowId IS NOT NULL AND pCurrentProgressOrder IS NOT NULL THEN
    -- Getting order for next progress
    SELECT F.`ProgressId`, F.`Order` INTO pNextProgressId, pNextProgressOrder FROM `Navigation.FlowDetails` AS F 
      WHERE `FlowId` = pFlowId AND `Order` > pCurrentProgressOrder LIMIT 1;

    IF pNextProgressId IS NOT NULL AND pNextProgressOrder IS NOT NULL THEN
      -- Next progress
      UPDATE `Navigation.AutoTasks`
        SET  `CurrentProgressId`    = pNextProgressId
            ,`CurrentProgressOrder` = pNextProgressOrder
            ,`NextToken`            = (SELECT MD5(CONCAT(pRobotId, " ", pTaskId, " ", pNextProgressId, " ", UTC_TIMESTAMP(6))))
            ,`UpdatedAt`            = UTC_TIMESTAMP(6)
        WHERE `Id` = pTaskId;
      SET pTaskUpdated = 1;
    ELSE
      -- Complete
      UPDATE `Navigation.AutoTasks`
        SET  `CurrentProgressId`    = 3
            ,`CurrentProgressOrder` = NULL
            ,`NextToken`            = NULL
            ,`UpdatedAt`            = UTC_TIMESTAMP(6)
        WHERE `Id` = pTaskId;
      SET pTaskUpdated = 1;
    END IF;
  END IF;
  COMMIT;

  IF pTaskUpdated = 1 THEN
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = pTaskId;
  ELSE
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = NULL;
  END IF;
END //

-- Abort Auto Task
DROP PROCEDURE IF EXISTS auto_task_abort //
CREATE PROCEDURE auto_task_abort (
   IN pRobotId CHAR(36)
  ,IN pTaskId INT
  ,IN pNextToken CHAR(32)
)
BEGIN
  DECLARE pTaskCount INT;
  DECLARE pTaskAborted INT DEFAULT 0;

  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    START TRANSACTION;
    ROLLBACK;
    RESIGNAL;
  END;

  START TRANSACTION;
  SELECT COUNT(*) INTO pTaskCount
    FROM `Navigation.AutoTasks` AS T
    WHERE T.`Id` = pTaskId AND T.`AssignedRobotId` = pRobotId AND T.`NextToken` = nextToken
    LIMIT 1 FOR UPDATE NOWAIT;
  
  IF pTaskCount = 1 THEN
    UPDATE `Navigation.AutoTasks`
      SET  `CurrentProgressId`    = 4
          ,`CurrentProgressOrder` = NULL
          ,`NextToken`            = NULL
          ,`UpdatedAt`            = UTC_TIMESTAMP(6)
      WHERE `Id` = pTaskId;
    SET pTaskAborted = 1;
  END IF;
  COMMIT;

  IF pTaskAborted = 1 THEN
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = pTaskId;
  ELSE
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = NULL;
  END IF;
END //

-- Manual Abort Auto Task (Aborted by user)
DROP PROCEDURE IF EXISTS auto_task_abort_manual //
CREATE PROCEDURE auto_task_abort_manual (
  IN pTaskId INT
)
BEGIN
  DECLARE pTaskCount INT;
  DECLARE pTaskAborted INT DEFAULT 0;

  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    START TRANSACTION;
    ROLLBACK;
    RESIGNAL;
  END;

  START TRANSACTION;
  SELECT COUNT(*) INTO pTaskCount
    FROM `Navigation.AutoTasks` AS T
    WHERE T.`Id` = pTaskId
    LIMIT 1 FOR UPDATE NOWAIT;
  
  IF pTaskCount = 1 THEN
    UPDATE `Navigation.AutoTasks`
      SET  `CurrentProgressId`    = 4
          ,`CurrentProgressOrder` = NULL
          ,`NextToken`            = NULL
          ,`UpdatedAt`            = UTC_TIMESTAMP(6)
      WHERE `Id` = pTaskId;
    SET pTaskAborted = 1;
  END IF;
  COMMIT;

  IF pTaskAborted = 1 THEN
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = pTaskId;
  ELSE
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = NULL;
  END IF;
END //

-- Remove Auto Task
DROP PROCEDURE IF EXISTS auto_task_delete //
CREATE PROCEDURE auto_task_delete (
  IN pTaskId INT
)
BEGIN
  DECLARE pProgressId INT;
  DECLARE pTaskDeleted INT DEFAULT 0;

  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    START TRANSACTION;
    ROLLBACK;
    RESIGNAL;
  END;

  START TRANSACTION;
  SELECT T.`CurrentProgressId` INTO pProgressId
    FROM `Navigation.AutoTasks` AS T
    WHERE T.`Id` = pTaskId
    LIMIT 1 FOR UPDATE NOWAIT;

  IF pProgressId = 1 OR pProgressId = 2 THEN
    DELETE FROM `Navigation.AutoTasks` WHERE `Id` = pTaskId;
    SET pTaskDeleted = 1;
  END IF;
  COMMIT;
  SELECT pTaskDeleted;
END //

delimiter ;