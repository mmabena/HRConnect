import axios from "axios";

const API_BASE = "http://localhost:5147/api/LeaveApplications";

export const getLeaveHistory = async (employeeId) => {
  const token = localStorage.getItem("token");

  const response = await axios.get(
    `${API_BASE}/by-employee-id/${employeeId}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );

  return response.data;
};
export const applyLeave = async (formData) => {
  const token = localStorage.getItem("token");

  const response = await axios.post(
    "http://localhost:5147/api/LeaveApplication",
    formData,
    {
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "multipart/form-data",
      },
    }
  );

  return response.data;
};