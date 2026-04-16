import React, {useState} from 'react';
import './DynamicGrid.css';
import useLocalCurrencyFormat from "../../../../../hooks/useLocalCurrencyFormat";

const DynamicGrid = ({
  data,
  columns,
  loading,
  error,
  onRowClick = null,
  currentPage,
  totalPages,
  onPageChange
}) => {
  const {
      toLocalCurrency
  } = useLocalCurrencyFormat();
  if (loading) return <div className="grid-loading">Loading ...</div>;
  if (error) return <div className="grid-error">Error loading data</div>;
  if (data.length === 0) return <div className="grid-empty">No data available</div>

  const handlePageChanges = (pageNumber) => {
    if(pageNumber >= 1 && pageNumber <= totalPages) {
        onPageChange(pageNumber);
    }
  };



  const getPageNumbers = () => {
    const pages = [];
    const maxPages = 5;
    let startPage = Math.max(1, currentPage - Math.floor(maxPages / 2));
    let endPage = Math.min(totalPages, startPage + maxPages - 1);

    if (endPage - startPage < maxPages - 1) {
        startPage = Math.max(1, endPage - maxPages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
    }
    return pages;
  };

  return(
    <div className="dynamic-grid-container">
      <div className="grid-wrapper">
        {/* Header Grid */}
        <div className="grid-header">
          {columns.map((col) => (
            <div
              key={col}
              className="grid-header-cell"
              data-width={col.width || 1}
            >
              {col.header}
            </div>
            ))}
        </div>

        {/* GGrid Body */}
        <div className="grid-body">
          {data.map((row, rowIndex) => (
            <div
              key={row.id || rowIndex}
              className={`grid-row ${onRowClick ? 'grid-row-clickable' : ''}`}
              onClick={() => onRowClick && onRowClick(row)}
            >
              {columns.map((col) => (
                <div
                  key={`${rowIndex}-${col.key}`}
                  className={`grid-cell grid-col-${col.key}`}
                >
                  {col.render
                    ? col.render(row[col.key], row)
                    : row[col.key]
                  }
                </div>
              ))}
            </div>
          ))}
        </div>
      </div>

      {/* Grid - Pagination */}
      <div className="grid-pagination">
        <button
          className="pagination-btn pagination-btn-first"
          onClick={() => handlePageChanges(1)}
          disabled={currentPage === 1}
        >
          First
        </button>

        <button
          className="pagination-btn pagination-btn-prev"
          onClick={() => handlePageChanges(currentPage - 1)}
        >
          Previous
        </button>

        <div
          className="pagination-number">
          {getPageNumbers().map((page) => (
            <button
              key={page}
              className={`pagination-number ${currentPage === page ? 'active' : ''}`}
              onClick={() => handlePageChanges(page)}
            >
                {page}
            </button>
          ))}
        </div>

        <button
          className="pagination-btn pagination-btn-next"
          onClick={() => handlePageChanges(currentPage + 1)}
          disabled={currentPage === totalPages}
        >
          Next
        </button>

        <button
          className="pagination-btn pagination-btn-last"
          onClick={() => handlePageChanges(totalPages)}
          disabled={currentPage === totalPages}
        >
          Last
        </button>

        <div className="pagination-info">
          <span>Page {currentPage} of {totalPages}</span>
        </div>

      </div>
    </div>
  );
};

export default DynamicGrid;