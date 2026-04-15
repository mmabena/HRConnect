import {MedicalAidOptionsProvider} from "./PayrollManagement/Deductions/MedicalAidOptions/MedicalAidOptionsContext";

export const AppProviders = ({ children }) => {
  return(
    <MedicalAidOptionsProvider>
      {children}
    </MedicalAidOptionsProvider>
  );
};

export default AppProviders;