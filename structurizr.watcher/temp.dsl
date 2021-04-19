workspace "Getting Started" "This is a model of my software system." {

    model {
        Auction_Attendee = person "Auction Attendee" "A member of the general public"
        Auctioneer = person "Auctioneer" "An auctioneer runs the auctions, accepting bids"
        TenantAdmin = person "Tenant Admin" "Tenant Administrator"
            
        enterprise "The Auction Collective" {        
            TacAdmin = person "TAC Administrator" "TAC Administrator"
            TACSystem = softwareSystem "TAC Portal" "The Auction Collective Portal"
        }
        
        Auction_Attendee -> TACSystem "Registers"
        Auction_Attendee -> TACSystem "Bids"
        Auction_Attendee -> TACSystem "Pays"

        Auctioneer -> TACSystem "Uses"
        TenantAdmin -> TACSystem "Uses"
        TacAdmin -> TACSystem "Uses"
    }

    views {
        systemContext TACSystem "SystemContext" "Tac Multi Tentant System. v0.0.1" {
            include *
            autoLayout
        }
    }
    
}