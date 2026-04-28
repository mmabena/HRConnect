import {useEffect, useMemo, useState, useCallback} from 'react';
import ReactDOM from 'react-dom';
import './MedicalAidOptionViewModal.css';
import formatToLocalCurrency from "../../../../../utils/formatToLocalCurrency";
import formatSalaryBracket from "../../../../../utils/formatSalaryBracket";
import medicalAidOptionDynamicCalculator from "../../../../../utils/medicalAidOptionDynamicCalculator";
import Divider from './Divider';



function MedicalAidOptionViewModal({isOpen, onClose, title, data = [], categories = [], categoryArray = [], onSave}) {

    // New Arch
    const [isEditing, setIsEditing] = useState(false);
    const [currentPage, setCurrentPage] = useState(0);
    const [edits, setEdits] = useState(new Map());
    const [touchedItems, setTouchedItems] = useState(new Set());
    const [isDirty, setIsDirty] = useState(false);
    const [isSaving,setIsSaving] = useState(false);

    const {
      calculatePrincipalTotal,
      calculateAdultTotal,
      calculateChildTotal,
      calculateChild2Total
    } = medicalAidOptionDynamicCalculator(formatToLocalCurrency);

    const handleToggleEdit = useCallback(() => {
        setIsEditing(prev => !prev);
    }, []);

    const handleFieldChange = useCallback((optionId, field, newValue) => {
        setEdits((prev) => {
            const next = new Map(prev);
            const optionEdits = next.get(optionId) ?? {};
            next.set(optionId, { ...optionEdits, [field]: newValue});
            return next;
        });
        setTouchedItems((prev) => {
            const next = new Set(prev);
            next.add(optionId);
            return next;
        })
        setIsDirty(true);
    }, []);

    const getEffectiveValue = useCallback((optionId, field, originalValue) => {
        const optionEdits = edits.get(optionId);
        if(optionEdits && optionEdits[field] !== undefined){
            return optionEdits[field];
        }
        return originalValue;
    }, [edits]);

    const groupedOptionsByCategory = (options, categories) => {
        const grouped = {};
        options.forEach(option => {
            const optionId = option.medicalOptionId;
            const categoryId = option.medicalOptionCategoryId;
            const category = categories.find(cat => cat.medicalOptionCategoryId === categoryId);
            const categoryName = category?.medicalOptionCategoryName ?? 'Unknown';

            if (!grouped[categoryId]) {
                grouped[categoryId] = {
                    categoryId,
                    categoryName,
                    options: []
                };
            }
            grouped[categoryId].options.push(option);
        });
        return Object.values(grouped);
    }

    const groupedData = useMemo(() =>
     groupedOptionsByCategory(data, categories), [data, categories]);

    const currentGroup = groupedData[0] ?? null;
    const totalOptions = currentGroup?.options.length ?? 0;
    const currentOption = currentGroup?.options[currentPage] ?? null;
    const canSave = isDirty && touchedItems.size === totalOptions && totalOptions > 0;

    useEffect(() => {
     if (isOpen) {
       setIsEditing(false);
       setCurrentPage(0);
       setEdits(new Map());
       setTouchedItems(new Set());
       setIsDirty(false);
       setIsSaving(false);
     }
    }, [isOpen, data]);

    const goToPage = useCallback((page) => {
       if(page >= 0 && page < totalOptions) setCurrentPage(page)
    }, [totalOptions]);

    const compilePayload = useCallback(() => {
      if(!currentGroup) return null;
      const  editedOptions = [];
      edits.forEach((changedFields, optionId) => {
        const originalOption = currentGroup.options.find(
          (o) => o.medicalOptionId === optionId
        );
        if(originalOption){
          editedOptions.push({
            medicalOptionId: optionId,
            medicalOptionName: originalOption.medicalOptionName,
            changedFields,
          });
        }
      });

     return {
       categoryId: currentGroup.categoryId,
       categoryName: currentGroup.categoryName,
       options: editedOptions
     };
    }, [edits, currentGroup]);

    const handleSave = useCallback(async () => {
      if(!canSave) return;
      setIsSaving(true);
      try{
        const payload = compilePayload();
        if(onSave && payload){
          await onSave(currentGroup.categoryId, payload);
        }
        setIsEditing(false);
        setEdits(new Map());
        setTouchedItems(new Set());
        setIsDirty(false);
      }
      catch (error) {
        console.error("Error saving changes:", error);
      }
      finally{
        setIsSaving(false);
      }
    }, [canSave, compilePayload, onSave, currentGroup]);

    // Close on Escape Key
    useEffect(() => {
        if(!isOpen) return;

        const handler = (e) => e.key === "Escape" && onClose();
        document.addEventListener("keydown", handler);
        return () => document.removeEventListener("keydown", handler);
    },[isOpen,onClose]);

    // Prevent background scroll when open | Scroll Lock
    useEffect(() => {
        document.body.style.overflowY = isOpen ? "hidden" : "";
        return () => {document.body.style.overflow = ""};
    }, [isOpen]);

    // Disable background interaction when open
    useEffect(() => {
        const appRoot = document.getElementById("root");

        if (!appRoot) return ;

        if(isOpen) {
          appRoot.setAttribute("inert", "");
        } else {
            appRoot.removeAttribute("inert");
        }
         return () => appRoot.removeAttribute("inert");

    },[isOpen]);

      if(!isOpen) return null;

      const fmtCurrency = (value) => {
        if(value === null || value === undefined) return '-';
        const num = Number(value);
        return num > 0 ? formatToLocalCurrency(num, "en-ZA") : '-';
      };

      const Cell = ({value, field, optionId, editable = false, type = 'text', className = '' }) => {
        if(isEditing && editable && field){
          return (
            <input
               className={`editable-cell-input ${className}`}
               type={type}
               value={value ?? ''}
               onChange={(e) => handleFieldChange(optionId, field, type === 'number' ? Number(e.target.value) : e.target.value)}
            />
          );
        }
        return <span className={`cell-value ${className}`}>{value ?? '-'}</span>
      };

      const renderOptionTable = () => {
        if(!currentOption) return null;
        const oid = currentOption.medicalOptionId;
        const editable = isEditing;

        const riskPrincipal = getEffectiveValue(oid, 'monthlyRiskContributionPrincipal', currentOption.monthlyRiskContributionPrincipal);
        const riskAdult = getEffectiveValue(oid, 'monthlyRiskContributionAdult', currentOption.monthlyRiskContributionAdult );
        const riskChild = getEffectiveValue(oid, 'monthlyRiskContributionChild', currentOption.monthlyRiskContributionChild);
        const riskChild2 = getEffectiveValue(oid, 'monthlyRiskContributionChild2', currentOption.monthlyRiskContributionChild2);
        const msaPrincipal = getEffectiveValue(oid, 'monthlyMsaContributionPrincipal', currentOption.monthlyMsaContributionPrincipal);
        const msaAdult = getEffectiveValue(oid, 'monthlyMsaContributionAdult', currentOption.monthlyMsaContributionAdult);
        const msaChild = getEffectiveValue(oid, 'monthlyMsaContributionChild', currentOption.monthlyMsaContributionChild);
        const msaChild2 = getEffectiveValue(oid, 'monthlyMsaContributionChild2', currentOption.monthlyMsaContributionChild2 ?? 0);

        const totalPrincipal = calculatePrincipalTotal(riskPrincipal, msaPrincipal);
        const totalAdult = calculateAdultTotal(riskAdult, msaAdult);
        const totalChild = calculateChildTotal(riskChild, msaChild);
        const totalChild2 = calculateChild2Total(riskChild2, msaChild2);

        return (
          <table className="model-view-table">
            <thead>
              <tr>
                <th className="model-view-table-header-cell">Component</th>
                <th className="model-view-table-header-cell">Principal</th>
                <th className="model-view-table-header-cell">Adult</th>
                <th className="model-view-table-header-cell">Child</th>
                <th className="model-view-table-header-cell">Child 2</th>
              </tr>
            </thead>

            <tbody>
              {/* Non-editable: Option Name*/}
              <tr>
                <td className="modal-view-label-cell">Option Name</td>
                <td colSpan='4'>
                  <Cell value={currentOption.medicalOptionName} editable={false} />
                </td>
              </tr>

              {/* Non-editable: Salary Bracket*/}
              <tr>
                <td className="modal-view-label-cell">
                  Income Category
                </td>
                <td colSpan='4'>
                  <Cell
                    value={formatSalaryBracket(
                      getEffectiveValue(oid, 'salaryBracketMin', currentOption.salaryBracketMin),
                      getEffectiveValue(oid, 'salaryBracketMax', currentOption.salaryBracketMax),
                      formatToLocalCurrency
                    )}
                    editable={false}
                    />
                </td>
              </tr>
              {/* Section: Risk*/}
              <tr className="modal-view-section-row">
                <td colSpan='5' className="modal-view-section-cell">
                  MONTHLY RISK CONTRIBUTION
                </td>
              </tr>
              <tr>
                <td className="modal-view-label-cell">Risk Allocation</td>
                <td><Cell value={editable ? riskPrincipal : fmtCurrency(riskPrincipal)} field="monthlyRiskContributionPrincipal" optionId={oid} editable={editable} type="number"/></td>
                <td><Cell value={editable ? riskAdult : fmtCurrency(riskAdult)} field="monthlyRiskContributionAdult" optionId={oid} editable={editable} type="number"/></td>
                <td><Cell value={editable ? riskChild : fmtCurrency(riskChild)} field="monthlyRiskContributionChild" optionId={oid} editable={editable} type="number"/></td>
                <td><Cell value={editable ? riskChild2 : fmtCurrency(riskChild2)} field="monthlyRiskContributionChild2" optionId={oid} editable={editable} type="number"/></td>
              </tr>
              {/* Section: MSA*/}
              <tr className="modal-view-section-row">
                <td colSpan='5' className="modal-view-section-cell">
                  MONTHLY SAVINGS ACCOUNT (MSA)
                </td>
              </tr>
              <tr>
                <td className="modal-view-label-cell">MSA Allocation</td>
                <td><Cell value={editable ? msaPrincipal : fmtCurrency(msaPrincipal)} field="monthlyMsaContributionPrincipal" optionId={oid} editable={editable} type="number"/></td>
                <td><Cell value={editable ? msaAdult : fmtCurrency(msaAdult)} field="monthlyMsaContributionAdult" optionId={oid} editable={editable} type="number"/></td>
                <td><Cell value={editable ? msaChild : fmtCurrency(msaChild)} field="monthlyMsaContributionChild" optionId={oid} editable={editable} type="number"/></td>
              </tr>
              {/* Section: Totals*/}
              <tr className="modal-view-section-row">
                <td colSpan='5' className="modal-view-section-cell">TOTAL MONTHLY DEDUCTION</td>
              </tr>
              <tr className="modal-view-totals-row">
                <td className="modal-view-label-cell">Total</td>
                <td className="modal-view-label-cell">{totalPrincipal}</td>
                <td className="modal-view-label-cell">{totalAdult}</td>
                <td className="modal-view-label-cell">{totalChild}</td>
                <td className="modal-view-label-cell">{totalChild2}</td>
              </tr>
            </tbody>

          </table>
        );

      };

    const renderPagination = () => (
        <div className="modal-pagination">
            <button className="pagination-btn" onClick={() => goToPage(0)} disabled={currentPage === 0}>First</button>
            <button className="pagination-btn" onClick={() => goToPage(currentPage - 1)} disabled={currentPage === 0}>Previous</button>
            <span className="pagination-info">
          Option {currentPage + 1} of {totalOptions}
                {isEditing && currentOption && touchedItems.has(currentOption.medicalOptionId) && (
                    <span className="touched-indicator"> ✓ Edited</span>
                )}
                {isEditing && currentOption && !touchedItems.has(currentOption.medicalOptionId) && (
                    <span className="untouched-indicator"> ✏ Needs edit</span>
                )}
        </span>
            <button className="pagination-btn" onClick={() => goToPage(currentPage + 1)} disabled={currentPage >= totalOptions - 1}>Next</button>
            <button className="pagination-btn" onClick={() => goToPage(totalOptions - 1)} disabled={currentPage >= totalOptions - 1}>Last</button>
        </div>
    );

      const renderEditProgress = () => {
        if(!isEditing) return null;
        const remaining = totalOptions - touchedItems.size;
        return (
          <div className="edit-progress">
            <div className="edit-progress-bar">
              <div className="edit-progress-fill" style={{width: `${(remaining / totalOptions) * 100}%`}}/>
            </div>
            <span className="edit-progress-text">
              {touchedItems.size} / {totalOptions} options edited
              {remaining > 0 ? ` - ${remaining} remaining before save` : ' - All edited, ready to save'}
            </span>
          </div>
        );
      };

      return ReactDOM.createPortal(
        <div className="medicalaid-menu-background" onClick={onClose}>
          <div className="model-wrapper" onClick={(e) => e.stopPropagation()}>
            {/* Header*/}
            <div className="modal-header-container">
              <div className="modal-header-main-text">{title}</div>
              <div className="modal-header-main-secondary-text">{currentGroup?.categoryName ?? ''}</div>
              <button className="modal-button-close" onClick={onClose} aria-label="Close">&times;</button>
            </div>
          {/* Body */}
          <div className="medicalAidOptions-modalBody">
            {data.length === 0 ? (
              <p className="medicalAidOptions-modalEmpty">No data available.</p>
            ) : (
              <>
                {/* Option Selector */}
                {isEditing ? (
                  <div className="option-selector">
                    <label>EDITING OPTION:</label>
                    <span className="option-name-edit">{currentOption.medicalOptionName ?? ''}</span>
                  </div>
                ) : (
                  <div className="option-selector">
                    <label htmlFor="option-dropdown">MEDICAL OPTION</label>
                    <select
                      id="option-dropdown"
                      value={currentOption?.medicalOptionId ?? ''}
                      onChange={(e) => {
                        const idx = currentGroup.options.findIndex(
                          (o) => o.medicalOptionId === Number(e.target.value)
                        );
                        if (idx >= 0) goToPage(idx);
                        }}
                      className="category-dropdown"
                    >
                      {currentGroup?.options.map((opt) => (
                        <option key={opt.medicalOptionId} value={opt.medicalOptionId}>
                          {opt.medicalOptionName}
                        </option>
                      ))}
                    </select>
                  </div>
                )}

                <div className="section-divider">
                  <p>MONTHLY CONTRIBUTION BREAKDOWN</p>
                  <Divider />
                </div>

                {renderOptionTable()}

                <Divider />

                {renderPagination()}

                {renderEditProgress()}
              </>
            )}
          </div>


          <div className="medicalAidOptions-modalFooter">
            <button className="btn btn-secondary" onClick={onClose}>
              Cancel
            </button>
            <button
              className={`btn ${isEditing ? 'btn-warning' : 'btn-primary'}`}
              onClick={handleToggleEdit}
            >
              {isEditing ? 'Cancel Edit' : 'Edit Plan'}
            </button>

            {isEditing && (
              <button
                className="btn btn-primary"
                disabled={!canSave || isSaving}
                onClick={handleSave}
              >
                {isSaving ? 'Saving...' : 'Save All Changes'}
              </button>
            )}
          </div>

          </div>
        </div>,
        document.body
      );

// Helper Functions
// A Function to format column headers into readable labels
function formatHeader(key) {
  return key
    .replace(/_/g, " ")                       // Replaces underscores with spaces (for snake_case)
      .replace(/([A-Z])/g, " $1")            // Adds space before capital letters (for camelCase)
    .replace(/^\w/, (c) => c.toUpperCase())   // Capitalize first Letter
    .trim();                                  // Trim out trailing spaces and whitspaces
}

}

export default MedicalAidOptionViewModal;