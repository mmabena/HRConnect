import { useState } from "react";

const usePagination = (data = [], defaultItemsPerPage = 10) => {
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage, setItemsPerPage] = useState(defaultItemsPerPage);

  const totalPages = Math.ceil(data.length / itemsPerPage);

  const indexOfLastItem = currentPage * itemsPerPage;
  const indexOfFirstItem = indexOfLastItem - itemsPerPage;

  const currentItems = data.slice(indexOfFirstItem, indexOfLastItem);

  const handlePrev = () => {
    if (currentPage > 1) {
      setCurrentPage((prev) => prev - 1);
    }
  };

  const handleNext = () => {
    if (currentPage < totalPages) {
      setCurrentPage((prev) => prev + 1);
    }
  };

  const handlePageClick = (num) => {
    setCurrentPage(num);
  };

  const changeItemsPerPage = (num) => {
    setItemsPerPage(num);
    setCurrentPage(1); // reset page
  };

  return {
    currentPage,
    itemsPerPage,
    totalPages,
    currentItems,
    setCurrentPage,
    handlePrev,
    handleNext,
    handlePageClick,
    changeItemsPerPage,
  };
};

export default usePagination;