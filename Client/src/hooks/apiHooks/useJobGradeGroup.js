import { useCallback } from "react";
import { toast } from "react-toastify";

import { getJobGradeGroups, 
    createJobGradeGroups, 
    updateJobGradeGroups, 
    deleteJobGradeGroup } from "../../api/JobGradeGroup";

const useJobGradeGroup = () => {
    const fetchJobGradeGroups = useCallback( async () => {
        try {
            const response = await getJobGradeGroups();

            return response;
        }
        catch (error) {
            console.error("Error fetching job grade groups:", error);
            throw error;
        }
    }, []);

    const addJobGradeGroups = useCallback( async (data) => {
        try {
            const response = await createJobGradeGroups(data);

            return response
        }
        catch (error) {
            console.error("Error creating job grade groups:", error);
        throw error;

        }
    }, []);

    const editJobGradeGroups = useCallback( async (data) => {
        try {
            const response = await updateJobGradeGroups(data);

            return response
        }
        catch (error) {
            console.error("Error updating  job grade groups:", error);
        throw error;

        }
    }, []);

    const removeJobGradeGroups = useCallback( async (id) => {
        try {
            const response = await deleteJobGradeGroup(id);

            return response
        }
        catch (error) {
            console.error("Error deleting job grade groups:", error);
        throw error;

        }
    }, []);

    return {fetchJobGradeGroups,
        addJobGradeGroups,
        editJobGradeGroups,
        removeJobGradeGroups,
    };
};

export default useJobGradeGroup;