import { useCallback } from "react";

import {
  getLeaveTypes,
  createLeaveType,
  updateLeaveType,
  updateLeaveRule,
  previewEntitlementImpact,
} from "../../api/leaveTypeApi";

const useLeaveType = () => {
  const fetchLeaveTypes = useCallback(async () => {
    try {
      const response = await getLeaveTypes();

      return response;
    } catch (error) {
      console.error("Error fetching leave types:", error);
      throw error;
    }
  }, []);

  const addLeaveType = useCallback(async (data) => {
    try {
      const response = await createLeaveType(data);

      return response;
    } catch (error) {
      console.error("Error creating leave type:", error);
      throw error;
    }
  }, []);

  const editLeaveType = useCallback(async (id, data) => {
    try {
      const response = await updateLeaveType(id, data);

      return response;
    } catch (error) {
      console.error("Error updating leave type:", error);
      throw error;
    }
  }, []);

  const editLeaveRule = useCallback(async (ruleId, newDays) => {
    try {
      const response = await updateLeaveRule(ruleId, newDays);

      return response;
    } catch (error) {
      console.error("Error updating rule:", error);
      throw error;
    }
  }, []);

  const previewEntitlement = useCallback(async (data) => {
    try {
      const response = await previewEntitlementImpact(data);

      return response;
    } catch (error) {
      console.error("Error previewing entitlement impact:", error);
      throw error;
    }
  }, []);

  return {
    fetchLeaveTypes,
    addLeaveType,
    editLeaveType,
    editLeaveRule,
    previewEntitlement,
  };
};

export default useLeaveType;
