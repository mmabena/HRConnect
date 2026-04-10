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