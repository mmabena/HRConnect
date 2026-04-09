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