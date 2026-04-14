import react, {useEffect, useState} from 'react';
import medicalOptionServices, {medicalOptionService} from '../../../../../Components/Services/medicalOptionServices';
import NavBar from "../../../../../Components/NavBar.jsx";
import {toast} from "react-toastify";

const MedicalAidOptionsManagement = () => {
  const [medicalOptions, setMedicalOptions] = useState([]);
  const [initialized, setInitialized] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  //use effects
  useEffect(() => {
    const initializeOptions = async () => {
      try{
        console.log("-----------=: Initialization of Medical options on mount :=------------");
        await medicalOptionServices.getMedicalOptionsSnapshot();
        setInitialized(true);
        console.log("-----------=: Medical Aid Options Data loaded :=------------");
      } catch (error) {
          console.error(`-----------=: Error Caught :=------------\n\n${error}`);
          setError(error);
          toast.error('Failed to load Medical Aid options!');
      }
    };

    initializeOptions();
  },[]);

  return (
      <>
          Test | Medical Aid Options Table will live here
      </>
  );
};


export default MedicalAidOptionsManagement;