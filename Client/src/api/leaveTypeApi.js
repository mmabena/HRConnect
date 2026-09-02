import api from "./api.js";

export const getLeaveTypes = async () => {
  const response = await api.get(`leave-types`);

  return response.data;
};

export const createLeaveType = async (data) => {
  const response = await api.post(`leave-types`, data);
  return response.data;
};
// UPDATE LEAVE TYPE (non-annual)
export const updateLeaveType = async (id, data) => {
  const response = await api.put(`leave-types/${id}`, data);
  return response.data;
};

// UPDATE RULE (annual leave)
export const updateLeaveRule = async (ruleId, newDays) => {
  const response = await api.put(`leave-rules/${ruleId}`, {
    ruleId,
    newDaysAllocated: newDays,
  });
  return response.data;
};

export const toggleLeaveTypeStatus = async (id) => {
  const response = await api.patch(`leave-types/${id}/status`, {});
  return response.data;
};

export const previewEntitlementImpact = async (data) => {
    const response = await api.post(
      `leave-types/preview-entitlement-impact`,
      data,);

    return response.data;
};
