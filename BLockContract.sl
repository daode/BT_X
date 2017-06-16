pragma solidity ^0.4.0;
contract BLockContract{
	
 
	
    //This Object will represent a transaction
	struct BLockTransaction {
		address seller;
		address buyer;
		address authority;
		bytes32 signatories;
		address lockID;
		address transcationID;
		bytes32 transactionStatus;
 
	}
	
	//A mapping where will be saved the transactions
	mapping(address => BLockTransaction) transactions;
	
	//Create a transaction and save it in the blockchain
	function createTransaction(address seller, address buyer, address  authority, bytes32 signatories, address lockID, address transcationID) returns (bool saved) {
		//store these informations in the blockchain.
		BLockTransaction memory tx = BLockTransaction(seller, buyer,authority,signatories,lockID, transcationID, "CREATED");
        transactions[transcationID] = tx;
		//We will return always true in this step.
		return true;
	}
 
	//Verify if the user is eligible to open the lock (is the user a part of the transaction?)
	function verifyUser(address transactionID) returns (bool verified) {
 
		BLockTransaction tx = transactions[transactionID];
 
		if(tx.seller == msg.sender || tx.buyer == msg.sender || tx.authority == msg.sender){
			return true;
		}else{
			return false;
		}
 
	}
 
 
	function cancelTransaction(address transactionID) returns (bool cancelled){
		transactions[transactionID].transactionStatus = "CANCELLED";
		return true;
	}
 
 
 
}
 
 
