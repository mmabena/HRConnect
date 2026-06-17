import axios from "axios";

const BASE_URL = "http://localhost:5147/api/job-grade-groups";

export const getJobGradeGroups = async () => {
    try {
        const response = await axios.get(BASE_URL, {
            headers: {
                Authorization: `Bearer ${localStorage.getItem("token")}`
            }
        });
        return response.data;

    }catch (error) {
        console.error("Error fetching job grade groups:", error);
        return [];
    }
};

export const createJobGradeGroups = async (data) => {
    try {
        const response = await axios.post(BASE_URL, data, {
            headers: {
                Authorization: `Bearer ${localStorage.getItem("token")}`,
                "Content-type": "application/json"
            }
        });
        return response.data
    }catch(error){
        console.error("Error creating job grade groups:", error);
        throw error;
    }
}

export const updateJobGradeGroups = async (data) => {
    try{
        const response = await axios.put(`${BASE_URL}/move`, data, {
           headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
          "Content-Type": "application/json",
        },
      }
    );

    return response.data;
  } catch (error) {
    console.error("Error updating job grade groups:", error);
    throw error;
  }
};

export const deleteJobGradeGroup = async (id) => {
  try {
    const response = await axios.delete(
      `${BASE_URL}?id=${id}`,
      {
        headers: {
          Authorization: `Bearer ${localStorage.getItem("token")}`,
          "Content-Type": "application/json",
        },
      }
    );

    return response.data;
  } catch (error) {
    console.error("Error deleting job grade group:", error);
    throw error;
  }
};