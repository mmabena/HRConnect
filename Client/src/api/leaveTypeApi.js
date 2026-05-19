import axios from "axios";

const BASE_URL = "http://localhost:5147/api/leave-types";

export const getLeaveTypes = async () => {
  try {
    const response = await axios.get(BASE_URL, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem("token")}`
      }
    });

    return response.data;
  } catch (error) {
    console.error("Error fetching leave types:", error);
    return [];
  }

};

export const createLeaveType = async (data) => {
  try {
    const response = await axios.post(BASE_URL, data, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem("token")}`,
        "Content-Type": "application/json"
      }
    });

    return response.data;
  } catch (error) {
    console.error("Error creating leave type:", error);
    throw error;
  }
};
// UPDATE LEAVE TYPE (non-annual)
export const updateLeaveType = async (id, data) => {
  try {
    const response = await axios.put(`${BASE_URL}/${id}`, data, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem("token")}`,
        "Content-Type": "application/json"
      }
    });
    return response.data;
  } catch (error) {
    console.error("Error updating leave type:", error);
    throw error;
  }
};

// UPDATE RULE (annual leave)
export const updateLeaveRule = async (ruleId, newDays) => {
  try {
    const response = await axios.put(
      `http://localhost:5147/api/leave-rules/${ruleId}`,
      {
        ruleId,
        newDaysAllocated: newDays
      },
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`
        }
      }
    );
    return response.data;
  } catch (error) {
    console.error("Error updating rule:", error);
    throw error;
  }
};

export const toggleLeaveTypeStatus = async (id) => {
  const response = await axios.patch(
    `http://localhost:5147/api/leave-types/${id}/status`,
    {},
    {
      headers: {
        Authorization: `Bearer ${localStorage.getItem("token")}`
      }
    }
  );

  return response.data;
};
export const previewEntitlementImpact = async (data) => {
  try {
    const response = await axios.post(
      `${BASE_URL}/preview-entitlement-impact`,
      data,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
          "Content-Type": "application/json"
        }
      }
    );

    return response.data;
  } catch (error) {
    console.error("Error previewing entitlement impact:", error);
    throw error;
  }
};