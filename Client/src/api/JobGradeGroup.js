import api from "./api.js";

export const getJobGradeGroups = async () => {
        const response = await api.get(`job-grade-groups`);
        return response.data;
};

export const createJobGradeGroups = async (data) => {
        const response = await api.post(`job-grade-groups`, data);
        return response.data
}

export const updateJobGradeGroups = async (data) => {
        const response = await api.put(`job-grade-groups/job-grades`, data);

    return response.data;
};

export const deleteJobGradeGroup = async (id) => {

    const response = await api.delete(`job-grade-groups?id=${id}`);

    return response.data;
};