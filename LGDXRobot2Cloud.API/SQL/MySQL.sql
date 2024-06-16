delimiter //

-- Assign Auto Task
DROP PROCEDURE IF EXISTS auto_task_assign_task //
CREATE PROCEDURE auto_task_assign_task (
  IN robotId CHAR(36)
)
BEGIN
  DECLARE taskId INT;
  DECLARE flowId INT;
  DECLARE progressId INT;
  DECLARE progressOrder INT;
  DECLARE runningTasks INT;
  
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    START TRANSACTION;
    ROLLBACK;
    RESIGNAL;
  END;

  -- Ensure no running task for robot
  SELECT COUNT(*) INTO runningTasks FROM `Navigation.AutoTasks` AS T
    WHERE T.`AssignedRobotId` = robotId
    AND T.`CurrentProgressId` != 1
    AND T.`CurrentProgressId` != 2
    AND T.`CurrentProgressId` != 8
    AND T.`CurrentProgressId` != 9;

  IF runningTasks = 0 THEN
    START TRANSACTION;
    SELECT T.`Id`, T.`FlowId` INTO taskId, flowId FROM `Navigation.AutoTasks` AS T 
      WHERE T.`CurrentProgressId` = 2 AND (T.`AssignedRobotId` = robotId OR T.`AssignedRobotId` IS NULL)
      ORDER BY T.`Priority` DESC, T.`AssignedRobotId` DESC, T.`Id`
      LIMIT 1 FOR UPDATE SKIP LOCKED;

    IF taskId IS NOT NULL AND flowId IS NOT NULL THEN
      SELECT F.`ProgressId`, F.`Order` INTO progressId, progressOrder FROM `Navigation.FlowDetails` AS F 
        WHERE `Id` = flowId ORDER BY `Order` LIMIT 1;

      UPDATE `Navigation.AutoTasks`
        SET  `AssignedRobotId`      = robotId
            ,`CurrentProgressId`    = progressId
            ,`CurrentProgressOrder` = progressOrder
            ,`CompleteToken`        = (SELECT MD5(CONCAT(robotId, " ", taskId, " ", progressId, " ", UTC_TIMESTAMP(6))))
            ,`UpdatedAt`            = UTC_TIMESTAMP(6)
        WHERE `Id` = taskId;
    END IF;
    COMMIT;
  END IF;

  IF taskId IS NOT NULL THEN
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = taskId;
  ELSE
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = NULL;
  END IF;
END //

-- Complete Progress
DROP PROCEDURE IF EXISTS auto_task_complete_progress //
CREATE PROCEDURE auto_task_complete_progress (
   IN robotId INT
  ,IN taskId INT
  ,IN completeToken CHAR(32)
)
BEGIN
  DECLARE flowId INT DEFAULT NULL;
  DECLARE currentProgressOrder INT DEFAULT NULL;
  DECLARE nextProgressId INT DEFAULT NULL;
  DECLARE nextProgressOrder INT DEFAULT NULL;
  DECLARE taskUpdated TINYINT DEFAULT 0;
  
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    START TRANSACTION;
    ROLLBACK;
    RESIGNAL;
  END;

  START TRANSACTION;
  SELECT T.`FlowId`, T.`CurrentProgressOrder` INTO flowId, currentProgressOrder 
    FROM `Navigation.AutoTasks` AS T
    WHERE T.`Id` = taskId AND T.`AssignedRobotId` = robotId AND T.`CompleteToken` = completeToken
    LIMIT 1 FOR UPDATE NOWAIT;

  IF flowId IS NOT NULL AND currentProgressOrder IS NOT NULL THEN
    -- Getting order for next progress
    SELECT F.`ProgressId`, F.`Order` INTO nextProgressId, nextProgressOrder FROM `Navigation.FlowDetails` AS F 
      WHERE `FlowId` = flowId AND `Order` > currentProgressOrder LIMIT 1;

    IF nextProgressId IS NOT NULL AND nextProgressOrder IS NOT NULL THEN
      -- Next progress
      UPDATE `Navigation.AutoTasks`
        SET  `CurrentProgressId`    = nextProgressId
            ,`CurrentProgressOrder` = nextProgressOrder
            ,`CompleteToken`        = (SELECT MD5(CONCAT(robotId, " ", taskId, " ", nextProgressId, " ", UTC_TIMESTAMP(6))))
            ,`UpdatedAt`            = UTC_TIMESTAMP(6)
        WHERE `Id` = taskId;
      SET taskUpdated = 1;
    ELSE
      -- Complete
      UPDATE `Navigation.AutoTasks`
        SET  `CurrentProgressId`    = 8
            ,`CurrentProgressOrder` = NULL
            ,`CompleteToken`        = NULL
            ,`UpdatedAt`            = UTC_TIMESTAMP(6)
        WHERE `Id` = taskId;
      SET taskUpdated = 1;
    END IF;
  END IF;
  COMMIT;

  IF taskUpdated = 1 THEN
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = taskId;
  ELSE
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = NULL;
  END IF;
END //

-- Abort Auto Task
DROP PROCEDURE IF EXISTS auto_task_abort //
CREATE PROCEDURE auto_task_abort (
   IN robotId CHAR(36)
  ,IN taskId INT
  ,IN completeToken CHAR(32)
)
BEGIN
  DECLARE taskCount INT;
  DECLARE taskAborted INT DEFAULT 0;

  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    START TRANSACTION;
    ROLLBACK;
    RESIGNAL;
  END;

  START TRANSACTION;
  SELECT COUNT(*) INTO taskCount
    FROM `Navigation.AutoTasks` AS T
    WHERE T.`Id` = taskId AND T.`AssignedRobotId` = robotId AND T.`CompleteToken` = completeToken
    LIMIT 1 FOR UPDATE NOWAIT;
  
  IF taskCount = 1 THEN
    UPDATE `Navigation.AutoTasks`
      SET  `CurrentProgressId`    = 9
          ,`CurrentProgressOrder` = NULL
          ,`CompleteToken`        = NULL
          ,`UpdatedAt`            = UTC_TIMESTAMP(6)
      WHERE `Id` = taskId;
    SET taskAborted = 1;
  END IF;
  COMMIT;

  IF taskAborted = 1 THEN
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = taskId;
  ELSE
    SELECT * FROM `Navigation.AutoTasks` AS T WHERE `Id` = NULL;
  END IF;
END //

-- Remove Auto Task
DROP PROCEDURE IF EXISTS auto_task_delete //
CREATE PROCEDURE auto_task_delete (
  IN taskId INT
)
BEGIN
  DECLARE progressId INT;
  DECLARE taskDeleted INT DEFAULT 0;

  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    START TRANSACTION;
    ROLLBACK;
    RESIGNAL;
  END;

  START TRANSACTION;
  SELECT T.`CurrentProgressId` INTO progressId
    FROM `Navigation.AutoTasks` AS T
    WHERE T.`Id` = taskId
    LIMIT 1 FOR UPDATE NOWAIT;

  IF progressId = 1 OR progressId = 2 THEN
    DELETE FROM `Navigation.AutoTasks` WHERE `Id` = taskId;
    SET taskDeleted = 1;
  END IF;
  COMMIT;
  SELECT taskDeleted;
END //

delimiter ;