-- Add SHA256 Transfers
INSERT INTO TransferTypeUsers 
SELECT '1', UserId
FROM Users
WHERE TransferAccount = 'True'

-- Add Case Sensitive Transfers
INSERT INTO TransferTypeUsers 
SELECT '2', UserId
FROM Users

-- Add ASCII Transfers
INSERT INTO TransferTypeUsers 
SELECT '3', UserId
FROM Users

INSERT INTO TransferTypePastes 
SELECT '3', PasteId
FROM Pastes
WHERE HashedPassword IS NOT NULL