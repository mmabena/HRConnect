import { createContext, useContext, useCallback, useEffect, useMemo, useState } from "react";
import medicalOptionServices from "../../../../../Components/Services/medicalOptionServices.js";

const MedicalAidOptions = createContext();

export const useMedicalAidOptionContext = () => {
    const context = useContext(MedicalAidOptions);

    if(!context) {
        throw new Error()('useMedicalAidOptionContext must be used within a MedicalAidOptionsProvider');
    }

    return context;
};

export const MedicalAidOptionsProvider = ({children}) => {
    const [medicalAidOptions, setMedicalAidOptions] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    // Api Service Layer Calls


};